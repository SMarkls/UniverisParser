using System.Windows;
using UniverisParser.Wpf.ViewModel;

namespace UniverisParser.Wpf.View;

public partial class JournalWindow : Window
{
    public JournalWindow(JournalViewModel viewModel)
    {
        InitializeComponent();
        MarksTable.DataContext = viewModel;
    }
    
}