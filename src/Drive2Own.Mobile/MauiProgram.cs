using Drive2Own.Mobile.Services;
using Drive2Own.Mobile.ViewModels;
using Drive2Own.Mobile.Views;
using Microsoft.Extensions.Logging;

namespace Drive2Own.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Services
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<AuthStateService>();
        builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<TrackingViewModel>();
        builder.Services.AddTransient<RoutesViewModel>();
        builder.Services.AddTransient<MapViewModel>();

        // Views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<TrackingPage>();
        builder.Services.AddTransient<RoutesPage>();
        builder.Services.AddTransient<MapPage>();

        // Shell
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
