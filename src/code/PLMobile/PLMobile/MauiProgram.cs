using Microsoft.Extensions.Logging;
using PLMobile.Services;
using PLMobile.ViewModels;
using PLMobile;

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

            // Register Services
            builder.Services.AddSingleton<ApiService>();

            // Register ViewModels
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<LibraryPageViewModel>();
            builder.Services.AddTransient<ImportPageViewModel>();
            builder.Services.AddTransient<BaseViewModel>();
            builder.Services.AddTransient<ReadPageViewModel>();
            builder.Services.AddTransient<TagsPageViewModel>();

            // Register Pages
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<LibraryPage>();
            builder.Services.AddTransient<ImportPage>();
            builder.Services.AddTransient<TagsPage>();
            builder.Services.AddTransient<LibraryPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
