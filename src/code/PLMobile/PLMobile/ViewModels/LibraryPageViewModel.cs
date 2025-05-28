using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PLMobile.Models;
using PLMobile.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace PLMobile.ViewModels
{
    public partial class LibraryPageViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private ObservableCollection<BookModel> _books;

        [ObservableProperty]
        private ObservableCollection<TagModel> _selectedTags;

        [ObservableProperty]
        private bool _isSortedByDateDesc;

        [ObservableProperty]
        private string _searchQuery;

        [ObservableProperty]
        private bool _isRefreshing;

        public LibraryPageViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Ma Bibliothèque";
            Books = new ObservableCollection<BookModel>();
            SelectedTags = new ObservableCollection<TagModel>();
            IsSortedByDateDesc = true;
            LoadBooksCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadBooks()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                IsRefreshing = true;

                var booksList = await _apiService.GetBooksAsync();
                
                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    booksList = booksList.Where(b => 
                        b.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                Books.Clear();
                var sortedBooks = IsSortedByDateDesc 
                    ? booksList.OrderByDescending(b => b.CreatedAt)
                    : booksList.OrderBy(b => b.CreatedAt);

                foreach (var book in sortedBooks)
                {
                    Books.Add(book);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur", "Impossible de charger les livres: " + ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task FilterByTag()
        {
            await Shell.Current.GoToAsync(nameof(TagsPage));
        }

        [RelayCommand]
        private async Task Search()
        {
            if (IsBusy) return;
            await LoadBooks();
        }

        [RelayCommand]
        private async Task ClearSearch()
        {
            if (IsBusy) return;
            SearchQuery = string.Empty;
            await LoadBooks();
        }

        [RelayCommand]
        private async Task SortByDate()
        {
            IsSortedByDateDesc = !IsSortedByDateDesc;
            await LoadBooks();
        }

        [RelayCommand]
        private async Task OpenBook(BookModel book)
        {
            if (book == null) return;

            var parameters = new Dictionary<string, object>
            {
                { "BookId", book.Id },
                { "Title", book.Title }
            };

            await Shell.Current.GoToAsync($"{nameof(ReadPage)}", parameters);
        }

        [RelayCommand]
        private async Task ManageTags()
        {
            await Shell.Current.GoToAsync(nameof(TagsPage));
        }

        [RelayCommand]
        private async Task ImportBook()
        {
            await Shell.Current.GoToAsync(nameof(ImportPage));
        }

        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
} 