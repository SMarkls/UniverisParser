using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UniverisParser.ViewModel;

namespace UniverisParser.Parser;

public class Parser
{
    private readonly MarkViewModel viewModel;
    private HttpClient client;
    public string Login { get; set; }
    public string Password { get; set; }

    public Parser(MarkViewModel viewModel)
    {
        this.viewModel = viewModel;
    }
    
    public async Task ParseAsync(string discipline, string controlPoint, string semestr, CancellationToken ct)
    {
        try
        {   
            client = InitClient();
            var verifyToken = await ConfigureVerifyTokenAsync(ct);
            if (string.IsNullOrEmpty(verifyToken))
                throw new ArgumentException("Неверный логин или пароль");
            await ConfigureNavMenuAsync(verifyToken, ct);
            var journalId = await FindJournalIdAsync(discipline, semestr, ct);
            if (string.IsNullOrEmpty(journalId))
                throw new ArgumentException("Неверное название дисциплины");
            var matches = await FindMarksAsync(controlPoint, journalId, ct);
            if (matches.Length < 3)
                throw new ArgumentException("Неверное название контрольной точки.", nameof(controlPoint));
            var point = matches.Groups[1].Value;
            var rating = matches.Groups[2].Value;
            viewModel.Point = point;
            viewModel.Rating = rating;
        }
        finally
        {
            client.Dispose();
        }
    }

    private async Task<Match> FindMarksAsync(string controlPoint, string journalId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var journalString = Application.Current.Resources["JournalString"] as string;
        journalString = string.Format(journalString, journalId).Replace("&amp;", "&");
        var response = await client.PostAsync("https://studlk.susu.ru/ru/StudyPlan/GetMarks",
            new StringContent(
                journalString,
                Encoding.UTF8, "application/x-www-form-urlencoded"), ct);
        var pageText = await response.Content.ReadAsStringAsync(ct);
        var markPattern =
            $@"Name"":""{controlPoint.Replace(" ", "[ ]*").Replace("(", @"\(").Replace(")", @"\)")}"",""EventId"":""[\w\d\-]+"",""TypeId"":""[\w\d\-]+""," + 
            @"""StudentId"":""[\d\w\-]+"",""Fio"":""[\w\d ]+"",""Point"":([\w\d\.]+),""Rating"":([\w\d\.]+),";
        var matches = Regex.Match(pageText, markPattern);
        return matches;
    }

    private async Task<string> FindJournalIdAsync(string discipline, string semestr, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var response = await client.GetAsync("https://studlk.susu.ru/ru/StudyPlan/StudyPlanGridPartialCustom", ct);
        var journalIdPattern =
            $@",""JournalId"":""([\w\d\-]*)"",""TermNumber"":[\d\w]+,""CycleName"":""[\d\w]+"",""DisciplineName"":""{discipline}""";
        var pageText = await response.Content.ReadAsStringAsync(ct);
        var matches = Regex.Matches(pageText, journalIdPattern);
        var journalId = matches[int.Parse(semestr)].Groups[1].Value;
        return journalId;
    }

    private async Task ConfigureNavMenuAsync(string verifyToken, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var navMenuString = Application.Current.Resources["NavMenuString"] as string;
        navMenuString = string.Format(navMenuString, verifyToken, Login, Password);
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
    
    private HttpClient InitClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Host = "studlk.susu.ru";
        return client;
    }
}