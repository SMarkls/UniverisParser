using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using UniverisParser.ViewModel;

namespace UniverisParser.View;

public partial class MainWindow
{
    private readonly CancellationTokenSource cts = new();
    private CancellationToken ct;
    private MarkViewModel viewModel;
    private readonly Parser.Parser parser;
    public MainWindow()
    {
        InitializeComponent();
        viewModel = (Resources["ViewModel"] as MarkViewModel)!;
        ct = cts.Token;
        parser = new Parser.Parser(viewModel);
        DataContext = viewModel;
    }

    private async void ParsingBtnClicked(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        btn!.Click -= ParsingBtnClicked;
        CancellBtn.IsEnabled = true;
        ParsingBtn.IsEnabled = false;
        ExceptionBlock.Text = "";
        parser.Login = LoginTextBox.Text;
        parser.Password = PasswordTextBox.Text;
        var discipline = DisciplineTextBox.Text;
        var controlPoint = ControlPointTextBox.Text;
        try
        {
            await parser.ParseAsync(discipline, controlPoint, ct);
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

    private void CancellationBtnClicked(object sender, RoutedEventArgs e)
    {
        cts.Cancel();
    }
}