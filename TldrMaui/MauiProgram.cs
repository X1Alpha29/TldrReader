using Microsoft.Maui.Hosting;
using TldrMaui.Services;
using TldrMaui.ViewModels;
using TldrMaui.Views;

namespace TldrMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // DI registrations
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<IFeedService, FeedService>();
        builder.Services.AddTransient<FeedViewModel>();
        builder.Services.AddTransient<FeedPage>(); // we'll add this page in the next step

        return builder.Build();
    }
}
