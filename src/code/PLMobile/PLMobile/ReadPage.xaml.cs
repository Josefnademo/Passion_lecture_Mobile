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

    public ReadPage(ApiService apiService, ReadPageViewModel viewModel)
    {
        InitializeComponent();
        _apiService = apiService;
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
    private async void GoBack(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}