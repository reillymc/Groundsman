using Groundsman.Helpers;
using Groundsman.Services;
using Groundsman.ViewModels;
using Groundsman.Pages;

namespace Groundsman;

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

        builder.Services.AddSingleton(new DatabaseService(Path.Combine(FileSystem.AppDataDirectory, Constants.DatabaseFile)));
        builder.Services.AddSingleton<FeatureService>();


        builder.Services.AddSingleton<FeaturesPage>();
        builder.Services.AddSingleton<FeaturesViewModel>();

        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddSingleton<SettingsViewModel>();

        builder.Services.AddTransient<EditPage>();
        builder.Services.AddTransient<EditViewModel>();

        builder.Services.AddTransient<AboutPage>();
        builder.Services.AddTransient<AddPage>();
        builder.Services.AddTransient<WelcomePage>();

        return builder.Build();
    }
}
