using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using PLMobile.Models;
using PLMobile.Services;
using System.Diagnostics;

namespace PLMobile.ViewModels
{
    [QueryProperty(nameof(BookId), "bookId")]
    public partial class BookTagsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        
        [ObservableProperty] private string bookTitle;
        [ObservableProperty] private int numericBookId;

        [ObservableProperty]
        private string bookId;


        [ObservableProperty] private ObservableCollection<TagItemViewModel> availableTags = new();
        [ObservableProperty] private ObservableCollection<TagItemViewModel> selectedTags = new();
        [ObservableProperty] private bool isTagDropdownVisible;
        [ObservableProperty] private TagItemViewModel selectedTag;
        [ObservableProperty] private string selectedTagText = "Select tags...";

        public BookTagsViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        public async Task LoadTags()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                var allTags = await _apiService.GetTagsAsync();
                var bookTags = await _apiService.GetBookTagsAsync(NumericBookId);



                AvailableTags.Clear();
                SelectedTags.Clear();

                foreach (var tag in allTags)
                {
                    var tagVm = new TagItemViewModel
                    {
                        Id = tag.Id,
                        Name = tag.Name
                    };

                    AvailableTags.Add(tagVm);

                    if (bookTags.Any(t => t.Id == tag.Id))
                    {
                        SelectedTags.Add(tagVm);
                    }
                }
                Debug.WriteLine($"Loading tags for book with numeric ID: {NumericBookId}");

                UpdateSelectedTagText();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load tags: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnSelectedTagChanged(TagItemViewModel value)
        {
            if (value != null && !SelectedTags.Any(t => t.Id == value.Id))
            {
                SelectedTags.Add(new TagItemViewModel
                {
                    Id = value.Id,
                    Name = value.Name
                });

                var toRemove = AvailableTags.FirstOrDefault(t => t.Id == value.Id);
                if (toRemove != null)
                    AvailableTags.Remove(toRemove);

                UpdateSelectedTagText();
            }

            IsTagDropdownVisible = false;
            SelectedTag = null;
        }


        [RelayCommand]
        private void ToggleTagDropdown()
        {
            IsTagDropdownVisible = !IsTagDropdownVisible;
        }

        [RelayCommand]
        private void RemoveTag(TagItemViewModel tag)
        {
            SelectedTags.Remove(tag);
            if (!AvailableTags.Any(t => t.Id == tag.Id))
            {
                AvailableTags.Add(new TagItemViewModel
                {
                    Id = tag.Id,
                    Name = tag.Name
                });
            }
            UpdateSelectedTagText();
        }


        private void UpdateSelectedTagText()
        {
            SelectedTagText = SelectedTags.Any()
                ? string.Join(", ", SelectedTags.Select(t => t.Name))
                : "Select tags...";
        }


        [RelayCommand]
        public async Task SaveTagsAsync()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(BookId))
            {
                await Shell.Current.DisplayAlert("Error", "Book ID is missing", "OK");
                return;
            }

            try
            {
                IsBusy = true;

                var selectedTagIds = SelectedTags
                    .Select(t => t.Id)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .ToList();

                var success = await _apiService.UpdateBookTagsAsync(BookId, selectedTagIds);

                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Tags updated successfully", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to update tags", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update tags: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }





    }
}
