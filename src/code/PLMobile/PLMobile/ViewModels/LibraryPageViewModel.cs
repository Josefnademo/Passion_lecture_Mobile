using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PLMobile.Models;
using PLMobile.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Json;

namespace PLMobile.ViewModels
{
    public partial class LibraryPageViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private readonly HttpClient _httpClient;

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

        [ObservableProperty]
        private string _activeTagFilter;

        [ObservableProperty]
        private int? _activeYearFilter;


        //new books on top
        public bool HasActiveTagFilter => !string.IsNullOrEmpty(ActiveTagFilter);
        public bool HasActiveYearFilter => ActiveYearFilter.HasValue;
        public bool HasAnyFilter => HasActiveTagFilter || HasActiveYearFilter;

        private List<BookModel> _allBooks;
        private List<TagModel> _availableTags;
        private HashSet<int> _availableYears;

        public LibraryPageViewModel(ApiService apiService, HttpClient httpClient)
        {
            _apiService = apiService;
            _httpClient = httpClient;
            Title = "Ma Bibliothèque";
            Books = new ObservableCollection<BookModel>();
            SelectedTags = new ObservableCollection<TagModel>();
            IsSortedByDateDesc = true;
            _allBooks = new List<BookModel>();
            _availableTags = new List<TagModel>();
            _availableYears = new HashSet<int>();
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

                _allBooks = await GetBooksAsync();
                _availableTags = await _apiService.GetTagsAsync();
                _availableYears = new HashSet<int>(_allBooks.Select(b => b.CreatedAt.Year));
                ApplyFilters();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur", $"Impossible de charger les livres: {ex.Message}", "OK");
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
        private void SortByDate()
        {
            IsSortedByDateDesc = !IsSortedByDateDesc;
            ApplyFilters();
        }

        [RelayCommand]
        private async Task ShowTagFilter()
        {
            if (_availableTags == null || !_availableTags.Any())
            {
                await Shell.Current.DisplayAlert("Info", "Aucun tag disponible", "OK");
                return;
            }

            var tagNames = _availableTags.Select(t => t.Name).ToList();
            tagNames.Insert(0, "Tous les tags");

            var selectedTag = await Shell.Current.DisplayActionSheet(
                "Sélectionner un tag", "Annuler", null, tagNames.ToArray());

            if (selectedTag != null && selectedTag != "Annuler")
            {
                ActiveTagFilter = selectedTag == "Tous les tags" ? null : selectedTag;
                ApplyFilters();
            }
        }

        [RelayCommand]
        private async Task ShowYearFilter()
        {
            if (_availableYears == null || !_availableYears.Any())
            {
                await Shell.Current.DisplayAlert("Info", "Aucune année disponible", "OK");
                return;
            }

            var years = _availableYears.OrderByDescending(y => y).Select(y => y.ToString()).ToList();
            years.Insert(0, "Toutes les années");

            var selectedYear = await Shell.Current.DisplayActionSheet(
                "Sélectionner une année", "Annuler", null, years.ToArray());

            if (selectedYear != null && selectedYear != "Annuler")
            {
                ActiveYearFilter = selectedYear == "Toutes les années" ? null : int.Parse(selectedYear);
                ApplyFilters();
            }
        }

        [RelayCommand]
        private void ClearFilters()
        {
            ActiveTagFilter = null;
            ActiveYearFilter = null;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filteredBooks = _allBooks;

            // Apply tag filter
            if (!string.IsNullOrEmpty(ActiveTagFilter))
            {
                var selectedTag = _availableTags.FirstOrDefault(t => 
                    string.Equals(t.Name, ActiveTagFilter, StringComparison.OrdinalIgnoreCase));
                
                if (selectedTag != null)
                {
                    filteredBooks = filteredBooks.Where(b => 
                        b.Tags != null && 
                        b.Tags.Any(tag => tag.Id == selectedTag.Id))
                        .ToList();
                }
            }

            // Apply year filter
            if (ActiveYearFilter.HasValue)
            {
                filteredBooks = filteredBooks.Where(b => 
                    b.CreatedAt.Year == ActiveYearFilter.Value).ToList();
            }

            // Apply sorting
            filteredBooks = IsSortedByDateDesc 
                ? filteredBooks.OrderByDescending(b => b.CreatedAt).ToList()
                : filteredBooks.OrderBy(b => b.CreatedAt).ToList();

            // Update the observable collection
            Books.Clear();
            foreach (var book in filteredBooks)
            {
                Books.Add(book);
            }

            // Log the current state
            System.Diagnostics.Debug.WriteLine($"[API] Applied filters and sorting - Books count: {Books.Count}");
            if (Books.Any())
            {
                var years = Books.Select(b => b.CreatedAt.Year).Distinct().OrderByDescending(y => y);
                System.Diagnostics.Debug.WriteLine($"[API] Years in filtered books: {string.Join(", ", years)}");
                System.Diagnostics.Debug.WriteLine($"[API] Sorting order: {(IsSortedByDateDesc ? "Newest first" : "Oldest first")}");
            }
        }

        [RelayCommand]
        private async Task OpenBook(BookModel book)
        {
            if (book == null)
            {
                System.Diagnostics.Debug.WriteLine("[Navigation] Cannot open book: book is null");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"[Navigation] Attempting to open book: {book.Id} - {book.Title}");
                
                var parameters = new Dictionary<string, object>
                {
                    { "BookId", book.Id },
                    { "Title", book.Title }
                };

                System.Diagnostics.Debug.WriteLine($"[Navigation] Parameters prepared: BookId={book.Id}, Title={book.Title}");
                
                // Use the Shell navigation pattern with absolute route
                var route = $"///ReadPage?BookId={Uri.EscapeDataString(book.Id)}&Title={Uri.EscapeDataString(book.Title)}";
                System.Diagnostics.Debug.WriteLine($"[Navigation] Navigation route: {route}");
                
                await Shell.Current.GoToAsync(route);
                System.Diagnostics.Debug.WriteLine("[Navigation] Navigation completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Navigation] Error navigating to ReadPage: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[Navigation] Stack trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Erreur", 
                    "Impossible d'ouvrir le livre. Veuillez réessayer.", "OK");
            }
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

        public async Task<List<BookModel>> GetBooksAsync(List<string> tagIds = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[API] Starting GetBooksAsync...");
                
                // First test the connection
                if (!await TestConnectionAsync())
                {
                    System.Diagnostics.Debug.WriteLine("[API] Connection test failed, throwing exception");
                    throw new HttpRequestException("Cannot connect to API server");
                }

                string url = "/api/books";
                if (tagIds != null && tagIds.Any())
                {
                    url = $"/api/books/filter?tagIds={string.Join(",", tagIds)}";
                }

                System.Diagnostics.Debug.WriteLine($"[API] Fetching books from: {url}");
                var response = await _httpClient.GetFromJsonAsync<List<BookModel>>(url);
                var books = response ?? new List<BookModel>();
                System.Diagnostics.Debug.WriteLine($"[API] Successfully fetched {books.Count} books");
                
                // Log the dates we received
                foreach (var book in books)
                {
                    System.Diagnostics.Debug.WriteLine($"[API] Book {book.Title}: Created at {book.CreatedAt}");
                }
                
                if (books.Any())
                {
                    var years = books.Select(b => b.CreatedAt.Year).Distinct().OrderByDescending(y => y);
                    System.Diagnostics.Debug.WriteLine($"[API] Available years: {string.Join(", ", years)}");
                }
                
                return books;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Error in GetBooksAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[API] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task<bool> TestConnectionAsync()
        {
            try
            {
                return await _apiService.TestConnectionAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Connection test failed: {ex.Message}");
                return false;
            }
        }
    }
} 