using Microcharts.Maui;
using Microsoft.Extensions.Logging;
using TokeroHomeWork.Application.Interfaces;
using TokeroHomeWork.Application.Repositories;
using TokeroHomeWork.Application.ViewModels;
using TokeroHomeWork.Views;

namespace TokeroHomeWork;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMicrocharts()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<ICryptoPricingRepository, CryptoPricingRepository>();
        
        builder.Services.AddTransient<PortfolioPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<LoginPage>();

        builder.Services.AddTransient<LoginPageViewModel>();
        builder.Services.AddTransient<PortfolioViewModel>();
        builder.Services.AddTransient<HomePageViewModel>();


#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}