using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PLMobile.Services;
using PLMobile.Models;

namespace PLMobile.ViewModels
{
    [QueryProperty("BookId", "BookId")]
    [QueryProperty("BookTitle", "Title")]
    public partial class ReadPageViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private string _bookId;

        [ObservableProperty]
        private string _bookTitle;

        [ObservableProperty]
        private int _currentPage;

        [ObservableProperty]
        private byte[] _epubContent;

        [ObservableProperty]
        private string _pageContent;

        [ObservableProperty]
        private bool _canGoNext;

        [ObservableProperty]
        private bool _canGoPrevious;

        public ReadPageViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Lecture";
            CurrentPage = 1;
            CanGoNext = true;
            CanGoPrevious = false;
        }

        partial void OnBookIdChanged(string value)
        {
            LoadBookCommand.Execute(null);
        }

        partial void OnBookTitleChanged(string value)
        {
            Title = value;
        }

        partial void OnCurrentPageChanged(int value)
        {
            CanGoPrevious = value > 1;
            UpdateProgressCommand.Execute(value);
        }

        [RelayCommand]
        private async Task LoadBook()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                EpubContent = await _apiService.GetBookEpubAsync(BookId);
                var book = (await _apiService.GetBooksAsync()).FirstOrDefault(b => b.Id == BookId);
                
                if (book != null)
                {
                    CurrentPage = book.LastReadPage > 0 ? book.LastReadPage : 1;
                    await LoadPageContent(CurrentPage);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur", "Impossible de charger le livre: " + ex.Message, "OK");
                await GoBack();
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task LoadPageContent(int pageNumber)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                PageContent = $"Page {pageNumber}";
                CurrentPage = pageNumber;
                await UpdateProgress(pageNumber);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur", "Impossible de charger la page: " + ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task UpdateProgress(int page)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                await _apiService.UpdateLastReadPageAsync(BookId, page);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur", "Impossible de mettre à jour la progression: " + ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task NextPage()
        {
            if (CanGoNext)
            {
                await LoadPageContent(CurrentPage + 1);
            }
        }

        [RelayCommand]
        private async Task PreviousPage()
        {
            if (CanGoPrevious)
            {
                await LoadPageContent(CurrentPage - 1);
            }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
} 