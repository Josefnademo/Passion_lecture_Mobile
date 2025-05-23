using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassionLecture.Services;
using System.Collections.ObjectModel;

namespace PassionLecture.ViewModels
{
    public partial class LibraryViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly INavigation _navigation;

        [ObservableProperty]
        private ObservableCollection<BookViewModel> books;

        [ObservableProperty]
        private bool isLoading;

        public LibraryViewModel(ApiService apiService, INavigation navigation)
        {
            _apiService = apiService;
            _navigation = navigation;
            books = new ObservableCollection<BookViewModel>();
            LoadBooksAsync().ConfigureAwait(false);
        }

        [RelayCommand]
        private async Task LoadBooks()
        {
            try
            {
                IsLoading = true;
                var apiBooks = await _apiService.GetBooksAsync();
                Books.Clear();
                foreach (var book in apiBooks)
                {
                    var coverImage = await _apiService.GetBookCoverAsync(book.Id);
                    var bookVm = new BookViewModel
                    {
                        Id = book.Id,
                        Title = book.Title,
                        CreatedAt = book.CreatedAt,
                        LastReadPage = book.LastReadPage,
                        CoverImageSource = ImageSource.FromStream(() => new MemoryStream(coverImage))
                    };
                    Books.Add(bookVm);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to load books: " + ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task FilterByTag()
        {
            var page = new TagSelectionPage();
            await _navigation.PushAsync(page);
            if (page.SelectedTags != null)
            {
                var tagIds = page.SelectedTags.Select(t => t.Id.ToString()).ToArray();
                var filteredBooks = await _apiService.GetBooksByTagAsync(tagIds);
                Books.Clear();
                foreach (var book in filteredBooks)
                {
                    var coverImage = await _apiService.GetBookCoverAsync(book.Id);
                    Books.Add(new BookViewModel
                    {
                        Id = book.Id,
                        Title = book.Title,
                        CreatedAt = book.CreatedAt,
                        LastReadPage = book.LastReadPage,
                        CoverImageSource = ImageSource.FromStream(() => new MemoryStream(coverImage))
                    });
                }
            }
        }

        [RelayCommand]
        private void SortByDate()
        {
            var sortedBooks = new ObservableCollection<BookViewModel>(
                Books.OrderByDescending(b => b.CreatedAt)
            );
            Books = sortedBooks;
        }
    }

    public partial class BookViewModel : ObservableObject
    {
        [ObservableProperty]
        private string id;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private DateTime createdAt;

        [ObservableProperty]
        private int lastReadPage;

        [ObservableProperty]
        private ImageSource coverImageSource;
    }
} 