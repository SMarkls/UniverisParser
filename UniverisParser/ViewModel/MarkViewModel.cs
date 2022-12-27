using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UniverisParser.ViewModel;

public class MarkViewModel : INotifyPropertyChanged
{
    private string point = "-1";
    private string rating = "-1";

    public string Point
    {
        get => point;
        set
        {
            point = value;
            OnPropertyChanged(nameof(MarkText));
        }
    }
    public string Rating
    {
        get => rating;
        set
        {
            rating = value;   
            OnPropertyChanged(nameof(MarkText));
        }
    }
    public string MarkText => $"Текущая оценка: {Point}.\nПроцентов: {Rating}";
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}