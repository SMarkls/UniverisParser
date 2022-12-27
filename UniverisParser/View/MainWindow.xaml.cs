using System.Windows;
using UniverisParser.ViewModel;

namespace UniverisParser.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MarkViewModel viewModel;
        private readonly Parser.Parser parser;
        public MainWindow()
        {
            InitializeComponent();
            viewModel = (Resources["ViewModel"] as MarkViewModel)!;
            parser = new Parser.Parser(viewModel);
            DataContext = viewModel;
        }

        private async void ParsingBtnClicked(object sender, RoutedEventArgs e)
        {
            parser.Login = LoginTextBox.Text;
            parser.Password = PasswordTextBox.Text;
            var discipline = DisciplineTextBox.Text;
            var controlPoint = ControlPointTextBox.Text;
            await parser.ParseAsync(discipline, controlPoint);
        }
    }
}