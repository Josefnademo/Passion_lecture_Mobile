using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace PLMobile;

public partial class TagsPage : ContentPage
{
    private readonly HttpClient _client = new();
    public ObservableCollection<TagModel> Tags { get; set; } = new();

    public TagsPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTags();
    }

    private async Task LoadTags()
    {
        try
        {
            var result = await _client.GetFromJsonAsync<List<TagModel>>("http://10.0.2.2:3000/tags");
            Tags.Clear();
            foreach (var tag in result)
                Tags.Add(tag);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", "Impossible de charger les tags : " + ex.Message, "OK");
        }
    }

    private async void CreateTag(object sender, EventArgs e)
    {
        string tagName = TagEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(tagName))
        {
            await DisplayAlert("Erreur", "Le nom du tag ne peut pas être vide", "OK");
            return;
        }

        var newTag = new { Name = tagName };
        var json = JsonConvert.SerializeObject(newTag);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("http://10.0.2.2:3000/tags", content);
        if (response.IsSuccessStatusCode)
        {
            TagEntry.Text = string.Empty;
            await LoadTags();
        }
        else
        {
            await DisplayAlert("Erreur", "Échec de la création du tag", "OK");
        }
    }



    private async void GoBack(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}

// Modèle de tag
public class TagModel
{
    public int Id { get; set; }
    public string Name { get; set; }
}
