using UniverisParser.Mobile.ViewModel;
using UniverisParser.ParserLibrary;
using UniverisParser.ParserLibrary.Model;

namespace UniverisParser.Mobile.View;

public partial class MainPage : ContentPage
{
    private Parser parser;
    private AppShell shell;
    private DisciplineViewModel vm;
    private CancellationTokenSource cts;
    private CancellationToken ct;
    public MainPage(Parser parser, AppShell shell)
    {
        InitializeComponent();
        DisciplineViewModel viewModel = Resources["ViewModel"] as DisciplineViewModel;
        vm = viewModel;
        this.parser = parser;
        this.shell = shell;
        cts = new();
        ct = cts.Token;
        
        vm!.Disciplines.Add(new Discipline { JournalId = "safsaf", Name = "safsaf", Semestr = "3" });
        vm.Disciplines.Add(new Discipline { JournalId = "ASDFSAF", Name = "asssssss", Semestr = "4" });
        vm.Disciplines.Add(new Discipline { JournalId = "hyhyhy", Name = "hahaha", Semestr = "5" });
        vm.Disciplines.Add(new Discipline { JournalId = "yeyeye", Name = "hohoho", Semestr = "3" });
    }

    private void CancellationBtnClicked(object sender, EventArgs e) => cts.Cancel();

    private async void ParsingBtnClicked(object sender, EventArgs e)
    {
        if (ct.IsCancellationRequested)
        {
            cts = new();
            ct = cts.Token;
        }

        if (!CheckForEmptyEntries())
        {
            await DisplayAlert("Ошибка", "Заполните все поля.", "Ок.");
            return;
        }
        var btn = sender as Button;
        vm.Disciplines = new();
        btn!.Clicked -= ParsingBtnClicked;
        CancelBtn.IsEnabled = true;
        ParsingBtn.IsEnabled = false;
        parser.Login = LoginEntry.Text;
        parser.Password = PasswordEntry.Text;
        var semestr = SemestrEntry.Text;
        try
        {
            var disciplines = await parser.FindAllDisciplinesInCurrentSemestrAsync(semestr, ct);
            vm.Disciplines = disciplines;
        }
        catch (OperationCanceledException)
        {
            await DisplayAlert("", "Парсинг прерван", "Ок.");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "Ок.");
        }
        finally
        {
            ParsingBtn.IsEnabled = true;
            CancelBtn.IsEnabled = false;
            btn.Clicked += ParsingBtnClicked;
        }
    }


    private bool CheckForEmptyEntries()
    {
        var result = CheckerOfBoxes(SemestrEntry);
        result &= CheckerOfBoxes(LoginEntry);
        result &= CheckerOfBoxes(PasswordEntry);
        return result;
    }

    private bool CheckerOfBoxes(Entry entry) => !string.IsNullOrEmpty(entry.Text);

    private async void DisciplineSelected(object sender, SelectedItemChangedEventArgs e)
    {
        var table = sender as ListView;
        table.ItemSelected -= DisciplineSelected;
        table.SelectedItem = null;

        var selectedItem = table.SelectedItem as Discipline;
        if (selectedItem == null) return;
        var points = await parser.FindAllControlPointsAsync(selectedItem.JournalId, ct);

        var journalViewModel = new JournalViewModel()
        {
            Points = points
        };
        var page = new JournalPage(shell, this);
        page.SetDataContext(journalViewModel);
        shell.CurrentItem = page;
        table.ItemSelected += DisciplineSelected;
    }
}