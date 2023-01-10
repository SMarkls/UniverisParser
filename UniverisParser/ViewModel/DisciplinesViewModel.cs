using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UniverisParser.Model;

namespace UniverisParser.ViewModel;
public class DisciplinesViewModel : INotifyPropertyChanged
{
    private List<Discipline> disciplines = new();
    public List<Discipline> Disciplines
    {
        get => disciplines;
        set
        {
            disciplines = value;
            OnPropertyChanged();
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}