using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UniverisParser.Model;
using UniverisParser.ViewModel;

namespace UniverisParser.View;

public partial class MainWindow
{
    private  CancellationTokenSource cts = new();
    private CancellationToken ct;
    private DisciplinesViewModel viewModel;
    private readonly Parser.Parser parser;
    public MainWindow()
    {
        InitializeComponent();
        viewModel = (Resources["ViewModel"] as DisciplinesViewModel)!;
        ct = cts.Token;
        parser = new Parser.Parser(viewModel);
        DataContext = viewModel;
    }

    private async void ParsingBtnClicked(object sender, RoutedEventArgs e)
    {
        if (ct.IsCancellationRequested)
        {
            cts = new();
            ct = cts.Token;
        }
        
        if (!CheckForEmptyTextBoxes())
        {
            ExceptionBlock.Text = "Заполните все поля.";
            return;
        }

        ExceptionBlock.Text = "";
        var btn = sender as Button;
        btn!.Click -= ParsingBtnClicked;
        CancellBtn.IsEnabled = true;
        ParsingBtn.IsEnabled = false;
        ExceptionBlock.Text = "";
        parser.Login = LoginTextBox.Text;
        parser.Password = PasswordTextBox.Text;
        var semestr = SemestrTextBox.Text;
        try
        {
            await parser.FindAllDisciplinesInCurrentSemestrAsync(semestr, ct);
        }
        catch (OperationCanceledException)
        {
            ExceptionBlock.Text = "Парсинг прерван.";
        } 
        catch (Exception ex)
        {
            ExceptionBlock.Text = ex.Message;
        }
        finally
        {
            ParsingBtn.IsEnabled = true;
            CancellBtn.IsEnabled = false;
            btn.Click += ParsingBtnClicked;
        }
    }

    private void CancellationBtnClicked(object sender, RoutedEventArgs e) => cts.Cancel();
    private void SemestrTextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (!int.TryParse(e.Key.ToString().Replace("D", ""), out int a))
            e.Handled = true;
        if (sender is TextBox { Text.Length: 1 })
            e.Handled = true;
    }

    private bool CheckForEmptyTextBoxes()
    {
        var result = CheckerOfBoxes(SemestrTextBox);
        result &= CheckerOfBoxes(LoginTextBox);
        result &= CheckerOfBoxes(PasswordTextBox);
        return result;
    }

    private bool CheckerOfBoxes(TextBox textBox)
    {
        if (string.IsNullOrEmpty(textBox.Text))
        {
            textBox.BorderBrush = new SolidColorBrush(Colors.DarkRed);
            textBox.MouseEnter += (_, _) => textBox.BorderBrush = new SolidColorBrush(Colors.Gray);
            return false;
        }

        return true;
    }

    private async void DisciplineGridDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var grid = sender as DataGrid;
        if (grid.Items.Count == 0)
            return;
        grid.MouseDoubleClick -= DisciplineGridDoubleClick;
        var journalId = (grid.SelectedItems[0] as Discipline).JournalId;
        var journalViewModel = await parser.FindAllControlPointsAsync(journalId, ct);
        grid.MouseDoubleClick += DisciplineGridDoubleClick;
        new JournalWindow(journalViewModel).ShowDialog();
    }
}