using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PLMobile.Models;
using PLMobile.Services;
using System.Collections.ObjectModel;

namespace PLMobile.ViewModels
{
    public partial class TagsPageViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private ObservableCollection<TagModel> _tags;

        [ObservableProperty]
        private string _newTagName;

        public TagsPageViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Gestion des tags";
            Tags = new ObservableCollection<TagModel>();
            LoadTagsCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadTags()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var tagsList = await _apiService.GetTagsAsync();
                Tags.Clear();
                foreach (var tag in tagsList)
                {
                    Tags.Add(tag);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur", "Impossible de charger les tags: " + ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CreateTag()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(NewTagName)) return;

            try
            {
                IsBusy = true;
                var newTag = await _apiService.CreateTagAsync(NewTagName);
                Tags.Add(newTag);
                NewTagName = string.Empty;
                await Shell.Current.DisplayAlert("Succès", "Tag créé avec succès", "OK");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("409"))
                {
                    await Shell.Current.DisplayAlert("Erreur", "Ce tag existe déjà", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Erreur", "Impossible de créer le tag: " + ex.Message, "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
} 