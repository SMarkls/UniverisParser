using System.Text;
using System.Text.RegularExpressions;
using UniverisParser.ParserLibrary.Model;

namespace UniverisParser.ParserLibrary;

/// <summary>
/// Класс парсера.
/// </summary>
public sealed class Parser
{
    private HttpClient client;
    /// <summary>
    /// Логин от системы "Универис".
    /// </summary>
    public string Login { get; set; }
    /// <summary>
    /// Пароль от системы "Универис".
    /// </summary>
    public string Password { get; set; }
    /// <summary>
    /// Находит все контрольные точки в дисциплине с данным ID журнала.
    /// </summary>
    /// <param name="journalId">ID журнала дисциплины.</param>
    /// <param name="ct">Токен отмены задачи.</param>
    /// <returns>Список всех контрольных точек.</returns>
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
    /// <summary>
    /// Находит все дисциплины, доступные студенту в данном семестре.
    /// </summary>
    /// <param name="semestr">Семестр, в котором нужно найти дисциплины.</param>
    /// <param name="ct">Токен отмены задачи.</param>
    /// <returns>Список всех дисциплин.</returns>
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
    /// <summary>
    /// Отправляет запрос на сервер Универиса и парсит из него список всех контрольных точек.
    /// </summary>
    /// <param name="journalId">ID Журнала.</param>
    /// <param name="ct">Токен отмены задачи.</param>
    /// <returns>Список контрольных точек.</returns>
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
            @"Name"":""([а-яА-Я\d\w\-(),. №]+)"",""EventId"":""[\w\d\-]+"",""TypeId"":""[\w\d\-]+"",""StudentId"":""[\d\w\-]+""" +
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
    /// <summary>
    /// Отправляет запрос на сервер универиса и парсит из него все дисциплины.
    /// </summary>
    /// <param name="semestr">Семестр, в котором нужно найти все дисциплины.</param>
    /// <param name="ct">Токен отмены задачи.</param>
    /// <returns>Список всех дисциплин.</returns>
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
    /// <summary>
    /// Добавляет в заголовки <see cref="HttpClient"/> заголовок 'nmbMainNavMenuBar', необходимый для работы с системой.
    /// </summary>
    /// <param name="verifyToken">Токен верификации.</param>
    /// <param name="ct">Токен отмены задачи.</param>
    /// <exception cref="ArgumentException">При неверных данных для входа выбрасывается исключение.</exception>
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
    /// <summary>
    /// Добавляет в заголовки <see cref="HttpClient"/> заголовок 'verifyToken', необходимый для работы с системой.
    /// </summary>
    /// <param name="ct">Токен заверщения задачи.</param>
    /// <returns>Токен верификации</returns>
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
    /// <summary>
    /// Вызывает методы конфигурации заголовков <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="ct">Токен отмены задачи.</param>
    /// <exception cref="ArgumentException">При неверных данных авторизации выбрасывает исключение.</exception>
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
    /// <summary>
    /// Создает объект <see cref="HttpClient"/> для запросов на сервер Универиса.
    /// </summary>
    /// <returns>Клиент для запросов на сервер Универиса.</returns>
    private HttpClient InitClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Host = "studlk.susu.ru";
        return client;
    }
}