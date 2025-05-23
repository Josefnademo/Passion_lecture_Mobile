using Microsoft.Extensions.Logging;
using PassionLecture.Services;
using PassionLecture.ViewModels;
using PassionLecture.Views;

namespace PassionLecture
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

            // Register pages and view models
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<LibraryPage>();
            builder.Services.AddTransient<LibraryViewModel>();
            builder.Services.AddTransient<TagsPage>();
            builder.Services.AddTransient<TagsViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
} 