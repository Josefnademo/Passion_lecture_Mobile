using EpubSharp;
using System.Net.Http;
using VersOne.Epub;

namespace PLMobile;

public partial class ReadPage : ContentPage
{
    private readonly HttpClient _client = new();
    private int _bookId;

    public ReadPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var uri = Shell.Current.CurrentState.Location;
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
            if (query.TryGetValue("id", out var idValue))
            {
                _bookId = int.Parse(idValue);
                await LoadBookContent();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", "Impossible de charger le livre : " + ex.Message, "OK");
        }
    }


    //Use the EpubSharp library to parse EPUB:
    private async Task LoadBookContent()
    {
        try
        {
            var response = await _client.GetAsync($"http://10.0.2.2:3000/epub/{_bookId}");
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                var epub = EpubReader.ReadBook(stream);

                // Affiche le contenu HTML du premier chapitre
                bookContent.Source = new HtmlWebViewSource
                {
                    Html = epub.Chapters.FirstOrDefault()?.HtmlContent ?? "<p>Aucun contenu</p>"
                };
            }
            else
            {
                await DisplayAlert("Erreur", "Échec du chargement EPUB", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", ex.Message, "OK");
        }
    }

    private async void GoBack(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}