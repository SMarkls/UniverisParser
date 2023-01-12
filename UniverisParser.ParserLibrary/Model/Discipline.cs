namespace UniverisParser.ParserLibrary.Model;
/// <summary>
/// Сущность дисциплины (предмета).
/// </summary>
public class Discipline
{
    /// <summary>
    /// Название дисциплины (предмета).
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// ID журнала. Используется для запроса на сервер для получения оценок.
    /// </summary>
    public string JournalId { get; set; }
    /// <summary>
    /// Семестр дисциплины.
    /// </summary>
    public string Semestr { get; set; }
}