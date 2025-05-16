using System.Net.Http.Json;
using System.Text;
namespace PLMobile;

public partial class LibraryPage : ContentPage
{
    private readonly HttpClient _client = new();
    private string _apiUrl = "http://10.0.2.2:3000/books";

    public LibraryPage()
    {
        InitializeComponent();
        LoadBooks();
    }

    private async void LoadBooks()
    {
        try
        {
            var response = await _client.GetAsync(_apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var books = await response.Content.ReadFromJsonAsync<List<Book>>();
                booksCollection.ItemsSource = books;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", ex.Message, "OK");
        }
    }

    private async void OnBookSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Book selectedBook)
        {
            await Shell.Current.GoToAsync($"{nameof(ReadPage)}?id={selectedBook.Id}");
        }
    }

    private async void GoBack(object sender, EventArgs e)
    {
        await AppShell.GoBackAsync();
    }
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; }
}
   
