using UniverisParser.Mobile.View;

namespace UniverisParser.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("JournalPage", typeof(JournalPage));
    }
}