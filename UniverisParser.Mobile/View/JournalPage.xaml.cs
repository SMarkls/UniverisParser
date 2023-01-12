using UniverisParser.Mobile.ViewModel;

namespace UniverisParser.Mobile.View;

public partial class JournalPage : ContentPage
{
    private readonly AppShell shell;
    private readonly MainPage page;

    public JournalPage(AppShell shell, MainPage page)
    {
        this.shell = shell;
        this.page = page;
        InitializeComponent();
    }

    public void SetDataContext(JournalViewModel vm) => ScrollView.BindingContext = vm;

    private void BackBtnClicked(object sender, EventArgs e)
    {
        shell.CurrentItem = page;
    }
}