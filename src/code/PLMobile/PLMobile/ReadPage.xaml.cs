using PLMobile.Services;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using PLMobile.ViewModels;
using PLMobile.Models;

namespace PLMobile;

public partial class ReadPage : ContentPage
{
    private readonly ApiService _apiService;
    private string _bookId;
    private int _currentPage = 1;
    private byte[] _epubData;
    private readonly ReadPageViewModel _viewModel;
    /*
    public string BookId
    {
        get => _bookId;
        set
        {
            _bookId = value;
            LoadBookAsync();
        }
    }*/

    public ReadPage(ApiService apiService, ReadPageViewModel viewModel)
    {
        InitializeComponent();
        _apiService = apiService;
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
    /*l
    private async void LoadBookAsync()
    {
        try
        {
            _epubData = await _apiService.GetBookEpubAsync(BookId);
            var book = (await _apiService.GetBooksAsync()).FirstOrDefault(b => b.Id == Book.Id);
            
            if (book != null)
            {
                _currentPage = book.LastReadPage;
                await LoadPage(_currentPage);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", "Impossible de charger le livre: " + ex.Message, "OK");
        }
    }

    private async Task LoadPage(int pageNumber)
    {
        try
        {
            // Here you would implement the actual EPUB rendering logic
            // For now, we'll just update the page number and save progress
            _currentPage = pageNumber;
            await _apiService.UpdateLastReadPageAsync(BookId, _currentPage);
            
            // Update UI to show current page
            pageLabel.Text = $"Page {_currentPage}";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", "Impossible de charger la page: " + ex.Message, "OK");
        }
    }

    private async void OnPreviousPageClicked(object sender, EventArgs e)
    {
        await _viewModel.UpdateProgressCommand.ExecuteAsync(_viewModel.CurrentPage - 1);
    }

    private async void OnNextPageClicked(object sender, EventArgs e)
    {
        await _viewModel.UpdateProgressCommand.ExecuteAsync(_viewModel.CurrentPage + 1);
    }*/

    private async void GoBack(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}