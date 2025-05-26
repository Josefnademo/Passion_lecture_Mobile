using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using PLMobile.Services;

namespace PLMobile.ViewModels
{
    public partial class MainPageViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public MainPageViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Passion Lecture";
        }

        [RelayCommand]
        private async Task TestConnection()
        {
            try
            {
                var response = await _apiService.TestConnectionAsync();
                string message = response ? 
                    "Successfully connected to API!" : 
                    "Could not connect to API. Make sure Docker is running.";
                    
                await Shell.Current.DisplayAlert(
                    "Connection Test", 
                    message, 
                    "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(
                    "Error", 
                    $"Error testing connection: {ex.Message}", 
                    "OK");
            }
        }

        [RelayCommand]
        private async Task NavigateToLibrary()
        {
            await Shell.Current.GoToAsync(nameof(LibraryPage));
        }

        [RelayCommand]
        private async Task NavigateToImport()
        {
            await Shell.Current.GoToAsync(nameof(ImportPage));
        }

        [RelayCommand]
        private async Task NavigateToTags()
        {
            await Shell.Current.GoToAsync(nameof(TagsPage));
        }
    }
} 