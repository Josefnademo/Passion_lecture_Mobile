using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PassionLecture.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly INavigation _navigation;

        public MainViewModel(INavigation navigation)
        {
            _navigation = navigation;
        }

        [RelayCommand]
        private async Task OpenLibrary()
        {
            await _navigation.PushAsync(new LibraryPage());
        }

        [RelayCommand]
        private async Task ImportBook()
        {
            await _navigation.PushAsync(new ImportBookPage());
        }

        [RelayCommand]
        private async Task OpenTags()
        {
            await _navigation.PushAsync(new TagsPage());
        }
    }
} 