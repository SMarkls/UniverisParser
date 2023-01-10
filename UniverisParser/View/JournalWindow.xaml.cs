using System.Windows;
using UniverisParser.ViewModel;

namespace UniverisParser.View;

public partial class JournalWindow : Window
{
    public JournalWindow(JournalViewModel viewModel)
    {
        InitializeComponent();
        MarksTable.DataContext = viewModel;
    }
    
}