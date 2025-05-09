//using BrowserEngineKit;
using Microsoft.Maui.Storage;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;


namespace PLMobile;

public partial class ImportPage : ContentPage
{
    private readonly HttpClient client = new();
    public ImportPage()
    {
        InitializeComponent();
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
            //display only EPUB files
            var options = new PickOptions
            {
                PickerTitle = "Select EPUB file", // File selection window title
                FileTypes = Epub // Limitation to EPUB files only (type of EPUB ,wich i created)
            };

            var file = await FilePicker.Default.PickAsync(options);

            if (file != null )
            {
                using var stream = await file.OpenReadAsync();
                using var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/epub+zip");

                content.Add(streamContent, "epub", file.FileName);

                var response = await client.PostAsync("http://localhost:3000/upload", content);

                if (response.IsSuccessStatusCode)
                    await DisplayAlert("Succès", "Livre importé avec succès.", "OK");
                else if ((int)response.StatusCode == 409)
                    await DisplayAlert("Attention", "Ce livre existe déjà.", "OK");
                else
                    await DisplayAlert("Erreur", "Impossible d'importer le livre.", "OK");
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
        await AppShell.GoBackAsync();
    }
}