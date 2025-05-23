//using BrowserEngineKit;
using Microsoft.Maui.Storage;
using PLMobile.Services;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using PLMobile.ViewModels;

namespace PLMobile;

public partial class ImportPage : ContentPage
{
    private readonly ApiService _apiService;

    public ImportPage(ApiService apiService, ImportPageViewModel viewModel)
    {
        InitializeComponent();
        _apiService = apiService;
        BindingContext = viewModel;
    }

    // This is the correct way to define file picker file types
    public static readonly FilePickerFileType Epub = new(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        //types of EPUB file on different OS
        { DevicePlatform.Android, new[] { "application/epub+zip" } },
        { DevicePlatform.iOS, new[] { "org.idpf.epub-container" } },
        { DevicePlatform.WinUI, new[] { ".epub" } }
    });

    private async void ImportBook(object sender, EventArgs e)//use NuGet "Plugin.FilePicker.Maui" cmd `dotnet add package Plugin.FilePicker.Maui`
    {
        //choose file, stock in db via api POST methode
        try
        {
            var options = new PickOptions
            {
                PickerTitle = "Select EPUB file",
                FileTypes = Epub
            };

            var file = await FilePicker.Default.PickAsync(options);

            if (file != null)
            {
                using var stream = await file.OpenReadAsync();
                var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var epubData = memoryStream.ToArray();

                try
                {
                    await _apiService.UploadBookAsync(
                        Path.GetFileNameWithoutExtension(file.FileName),
                        epubData,
                        null // We'll add cover image support later
                    );
                    await DisplayAlert("Succès", "Livre importé avec succès.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("409"))
                {
                    await DisplayAlert("Attention", "Ce livre existe déjà.", "OK");
                }
                catch
                {
                    await DisplayAlert("Erreur", "Impossible d'importer le livre.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", ex.Message, "OK");
        }
    }

    //Use GoBackAsync function from Shell to get on previous page
    private async void GoBack(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}