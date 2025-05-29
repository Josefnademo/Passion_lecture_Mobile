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

            // Register HttpClient
            builder.Services.AddSingleton<HttpClient>(serviceProvider =>
            {
                string baseUrl;
#if ANDROID
                // When using Android Emulator, we need to use 10.0.2.2 which maps to host's localhost
                baseUrl = "http://10.0.2.2:3000";
#elif WINDOWS
                baseUrl = "http://localhost:3000";
#else
                baseUrl = "http://localhost:3000";
#endif
                System.Diagnostics.Debug.WriteLine($"[API] Creating HttpClient with base URL: {baseUrl}");

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                var client = new HttpClient(handler)
                {
                    BaseAddress = new Uri(baseUrl),
                    Timeout = TimeSpan.FromSeconds(30)
                };

                // Add some default headers
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                return client;
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
            builder.Services.AddTransient<ApiPageViewModel>();

            // Register Pages
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<LibraryPage>();
            builder.Services.AddTransient<ImportPage>();
            builder.Services.AddTransient<TagsPage>();
            builder.Services.AddTransient<ApiPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
