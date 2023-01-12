namespace UniverisParser.ParserLibrary.Model;
/// <summary>
/// Сущеость контрольной точки
/// </summary>
public class ControlPoint
{
    /// <summary>
    /// Название контрольной точки.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Оценка.
    /// </summary>
    public string Mark { get; set; }
    /// <summary>
    /// Рейтинг, то есть проценты.
    /// </summary>
    public string Rating { get; set; }
}