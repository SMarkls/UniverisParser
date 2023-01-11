using System.Text;
using System.Text.RegularExpressions;
using UniverisParser.ParserLibrary.Model;

namespace UniverisParser.ParserLibrary;

public class Parser
{
    private HttpClient client;
    public string Login { get; set; }
    public string Password { get; set; }

    public Parser()
    {
        
    }
    public async Task<List<ControlPoint>> FindAllControlPointsAsync(string journalId, CancellationToken ct)
    {
        try
        {
            await ConfigureAuthTokensAsync(ct);
            return await ParseAllControlPointAsync(journalId, ct);
        }
        finally
        {
            client.Dispose();
        }
    }

    public async Task<List<Discipline>> FindAllDisciplinesInCurrentSemestrAsync(string semestr, CancellationToken ct)
    {
        try
        {
            await ConfigureAuthTokensAsync(ct);
            return await ParseAllDisciplinesAsync(semestr, ct);
        }
        finally
        {
            client.Dispose();
        }
    }

    private async Task<List<ControlPoint>> ParseAllControlPointAsync(string journalId, CancellationToken ct)
    {
        List<ControlPoint> points = new();
        ct.ThrowIfCancellationRequested();
        var journalString = ParserResources.JournalString;
        journalString = string.Format(journalString, journalId).Replace("&amp;", "&");
        var response = await client.PostAsync("https://studlk.susu.ru/ru/StudyPlan/GetMarks",
            new StringContent(
                journalString,
                Encoding.UTF8, "application/x-www-form-urlencoded"), ct);
        var responseText = await response.Content.ReadAsStringAsync(ct);
        var markPattern =
            @"Name"":""([а-яА-Я\d\-(),. №]+)"",""EventId"":""[\w\d\-]+"",""TypeId"":""[\w\d\-]+"",""StudentId"":""[\d\w\-]+""" +
            @",""Fio"":""[а-яА-Я\w\d ]+"",""Point"":([\w\d\.]+),""Rating"":([\w\d\.]+)";
        var matches = Regex.Matches(responseText, markPattern);
        foreach (Match match in matches)
        {
            points.Add(new ControlPoint
            {
                Name = match.Groups[1].Value,
                Mark = match.Groups[2].Value,
                Rating = match.Groups[3].Value
            });
        }

        return points;
    }
    private async Task<List<Discipline>> ParseAllDisciplinesAsync(string semestr, CancellationToken ct)
    {
        List<Discipline> disciplines = new();
        ct.ThrowIfCancellationRequested();
        var response = await client.GetAsync("https://studlk.susu.ru/ru/StudyPlan/StudyPlanGridPartialCustom", ct);
        var responseText = await response.Content.ReadAsStringAsync(ct);
        var disciplinesPattern =
            $@",""JournalId"":""([\w\d\-]*)"",""TermNumber"":{semestr},""CycleName"":""[\d\w]+"",""DisciplineName"":""([а-яА-Я\- ]*)""";
        ct.ThrowIfCancellationRequested();
        var matches = Regex.Matches(responseText, disciplinesPattern);
        foreach (Match match in matches)
        {
            if (match.Groups[1].Value == "00000000-0000-0000-0000-000000000000")
                continue;
            disciplines.Add(new Discipline
            { 
                Name = match.Groups[2].Value,
                JournalId = match.Groups[1].Value,
                Semestr = semestr
            });
        }
        return disciplines;
    }

    private async Task ConfigureNavMenuAsync(string verifyToken, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        string navMenuString = ParserResources.NavMenuString;
        navMenuString = string.Format(navMenuString, verifyToken, Login, Password);
        navMenuString = navMenuString.Replace(@"amp;", "");
        var response = await client.PostAsync("https://studlk.susu.ru/ru/Account/Login",
            new StringContent(
                navMenuString,
                Encoding.UTF8, "application/x-www-form-urlencoded"), ct);
        var mainNavMenuCookie = response.Headers.FirstOrDefault(x => x.Key == "Set-Cookie");
        var navMenuCookiePattern = @"nmbMainNavMenuBar=([^;.]*);";
        if (mainNavMenuCookie.Value is null) throw new ArgumentException("Неверный логин или пароль");
        string navMenuCookieValue = Regex.Match(mainNavMenuCookie.Value.First(), navMenuCookiePattern).Groups[1].Value;
        client.DefaultRequestHeaders.Add("nmbMainNavMenuBar", navMenuCookieValue);
    }

    private async Task<string> ConfigureVerifyTokenAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        string verifyTokenPattern = @"name=""__RequestVerificationToken"" type=""hidden"" value=""(.*)"" />";
        var response = await client.GetAsync("https://studlk.susu.ru/Account/Login?ReturnUrl=%2Fru", ct);
        string pageText = Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync(ct));
        string verifyToken = Regex.Match(pageText, verifyTokenPattern).Groups[1].Value;
        client.DefaultRequestHeaders.Add("__RequestVerificationToken", verifyToken);
        return verifyToken;
    }

    private async Task ConfigureAuthTokensAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        client = InitClient();
        var verifyToken = await ConfigureVerifyTokenAsync(ct);
        if (string.IsNullOrEmpty(verifyToken))
            throw new ArgumentException("Неверный логин или пароль");
        ct.ThrowIfCancellationRequested();
        await ConfigureNavMenuAsync(verifyToken, ct);
    }
    private HttpClient InitClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Host = "studlk.susu.ru";
        return client;
    }
}