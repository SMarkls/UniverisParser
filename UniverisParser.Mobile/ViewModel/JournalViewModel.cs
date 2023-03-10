using System.ComponentModel;
using System.Runtime.CompilerServices;
using UniverisParser.ParserLibrary.Model;

namespace UniverisParser.Mobile.ViewModel;

public class JournalViewModel : INotifyPropertyChanged
{
    private List<ControlPoint> points;

    public List<ControlPoint> Points
    {
        get => points;
        set
        {
            points = value;
            OnPropertyChanged();
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}