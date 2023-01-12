using Microsoft.Extensions.Logging;
using UniverisParser.ParserLibrary;
using UniverisParser.Mobile.View;


namespace UniverisParser.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();
        builder.Services
            .AddTransient<Parser>()
            .AddSingleton<MainPage>()
            .AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}