using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UniverisParser.ParserLibrary;
using UniverisParser.ParserLibrary.Model;
using UniverisParser.Wpf.ViewModel;

namespace UniverisParser.Wpf.View;

public partial class MainWindow
{
    private  CancellationTokenSource cts = new();
    private CancellationToken ct;
    private DisciplinesViewModel viewModel;
    private readonly Parser parser;
    public MainWindow()
    {
        InitializeComponent();
        viewModel = (Resources["ViewModel"] as DisciplinesViewModel)!;
        ct = cts.Token;
        parser = new Parser();
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
            var disciplines = await parser.FindAllDisciplinesInCurrentSemestrAsync(semestr, ct);
            viewModel.Disciplines = disciplines;
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
        var journalId = (grid.SelectedItems[0] as Discipline)!.JournalId;
        var controlPoints = await parser.FindAllControlPointsAsync(journalId, ct);
        var journalViewModel = new JournalViewModel
        {
            Points = controlPoints
        };
        grid.MouseDoubleClick += DisciplineGridDoubleClick;
        new JournalWindow(journalViewModel).ShowDialog();
    }

    private void FormKeyDown(object sender, KeyEventArgs e)
    {
        if(e.Key.ToString() == "Return")
            ParsingBtnClicked(ParsingBtn, new RoutedEventArgs());
    }

    private void MainFormClosing(object? sender, CancelEventArgs e)
    {
        string writingString =
            $@"""Login"": ""{LoginTextBox.Text}"", ""Password"": ""{PasswordTextBox.Text}"", ""Semestr"": ""{SemestrTextBox.Text}""";
        using FileStream fs = new FileStream("config.txt", FileMode.OpenOrCreate);
        byte[] bytes = Encoding.UTF8.GetBytes(writingString);
        fs.Write(bytes, 0, bytes.Length);
    }

    private void MainFormLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var credentials = File.ReadAllText("config.txt");
            var loginMatch = Regex.Match(credentials, @"""Login"": ""([\w\d]+)""");
            var passwordMatch = Regex.Match(credentials, @"""Password"": ""([\w\d]+)""");
            var semestrMatch = Regex.Match(credentials, @"""Semestr"": ""(\d+)""");
            var semestr = semestrMatch.Groups[1].Value;
            var login = loginMatch.Groups[1].Value;
            var password = passwordMatch.Groups[1].Value;
            PasswordTextBox.Text = password;
            LoginTextBox.Text = login;
            SemestrTextBox.Text = semestr;
        }
        catch (IOException)
        {
            
        }
    }
}