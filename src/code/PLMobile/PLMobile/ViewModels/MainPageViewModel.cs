using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PLMobile.ViewModels
{
    public partial class MainPageViewModel : BaseViewModel
    {
        public MainPageViewModel()
        {
            Title = "Passion Lecture";
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