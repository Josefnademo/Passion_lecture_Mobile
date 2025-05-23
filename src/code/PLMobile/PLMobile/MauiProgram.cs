using Microsoft.Extensions.Logging;
using PLMobile.Services;
using PLMobile.ViewModels;

namespace PLMobile
{
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

            // Register services
            builder.Services.AddSingleton<ApiService>();

            // Register ViewModels
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<LibraryPageViewModel>();
            builder.Services.AddTransient<ImportPageViewModel>();
            builder.Services.AddTransient<TagsPageViewModel>();
            builder.Services.AddTransient<ReadPageViewModel>();

            // Register Pages
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<LibraryPage>();
            builder.Services.AddTransient<ImportPage>();
            builder.Services.AddTransient<TagsPage>();
            builder.Services.AddTransient<ReadPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
