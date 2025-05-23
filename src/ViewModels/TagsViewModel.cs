using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassionLecture.Services;
using System.Collections.ObjectModel;

namespace PassionLecture.ViewModels
{
    public partial class TagsViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly INavigation _navigation;

        [ObservableProperty]
        private ObservableCollection<Tag> tags;

        [ObservableProperty]
        private string newTagName;

        public TagsViewModel(ApiService apiService, INavigation navigation)
        {
            _apiService = apiService;
            _navigation = navigation;
            tags = new ObservableCollection<Tag>();
            LoadTagsAsync().ConfigureAwait(false);
        }

        [RelayCommand]
        private async Task LoadTags()
        {
            try
            {
                var apiTags = await _apiService.GetTagsAsync();
                Tags.Clear();
                foreach (var tag in apiTags)
                {
                    Tags.Add(tag);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to load tags: " + ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task CreateTag()
        {
            if (string.IsNullOrWhiteSpace(NewTagName))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please enter a tag name", "OK");
                return;
            }

            try
            {
                var newTag = await _apiService.CreateTagAsync(NewTagName);
                Tags.Add(newTag);
                NewTagName = string.Empty;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to create tag: " + ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            await _navigation.PopAsync();
        }
    }
} 