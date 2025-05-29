using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PLMobile.Services;
using PLMobile.Models;
using System.Text;
using System.IO.Compression;

namespace PLMobile.ViewModels
{
    [QueryProperty(nameof(BookId), "BookId")]
    [QueryProperty(nameof(BookTitle), "Title")]
    public partial class ReadPageViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private ZipArchive _epubArchive;
        private List<string> _pages;

        [ObservableProperty]
        private string _bookId;

        [ObservableProperty]
        private string _bookTitle;

        [ObservableProperty]
        private int _currentPage;

        [ObservableProperty]
        private string _pageContent;

        [ObservableProperty]
        private bool _canGoNext;

        [ObservableProperty]
        private bool _canGoPrevious;

        [ObservableProperty]
        private int _totalPages;

        public ReadPageViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _pages = new List<string>();
            Title = "Lecture";
            CurrentPage = 1;
            CanGoNext = false;
            CanGoPrevious = false;
        }

        partial void OnBookIdChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                LoadBookCommand.Execute(null);
            }
        }

        partial void OnBookTitleChanged(string value)
        {
            Title = value;
        }

        partial void OnCurrentPageChanged(int value)
        {
            CanGoPrevious = value > 1;
            CanGoNext = value < TotalPages;
            UpdateProgressCommand.Execute(value);
        }

        [RelayCommand]
        private async Task LoadBook()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                System.Diagnostics.Debug.WriteLine($"[ReadPage] Starting to load book with ID: {BookId}");
                
                // Get the EPUB content
                System.Diagnostics.Debug.WriteLine("[ReadPage] Requesting EPUB content from API");
                var epubData = await _apiService.GetBookEpubAsync(BookId);
                System.Diagnostics.Debug.WriteLine($"[ReadPage] Received EPUB data: {epubData?.Length ?? 0} bytes");

                if (epubData == null || epubData.Length == 0)
                {
                    throw new Exception("No EPUB content received");
                }

                using var stream = new MemoryStream(epubData);
                _epubArchive = new ZipArchive(stream, ZipArchiveMode.Read);
                System.Diagnostics.Debug.WriteLine("[ReadPage] Created ZipArchive from EPUB data");
                
                // Extract and process EPUB content
                System.Diagnostics.Debug.WriteLine("[ReadPage] Starting to extract pages");
                _pages = ExtractPages(_epubArchive);
                System.Diagnostics.Debug.WriteLine($"[ReadPage] Extracted {_pages.Count} pages");
                
                if (_pages.Count == 0)
                {
                    throw new Exception("No readable content found in EPUB");
                }

                TotalPages = _pages.Count;
                
                // Load last read page or start from beginning
                System.Diagnostics.Debug.WriteLine("[ReadPage] Getting book details for last read page");
                var book = (await _apiService.GetBooksAsync()).FirstOrDefault(b => b.Id == BookId);
                CurrentPage = book?.LastReadPage > 0 ? book.LastReadPage : 1;
                System.Diagnostics.Debug.WriteLine($"[ReadPage] Set current page to: {CurrentPage}");
                
                await LoadPageContent(CurrentPage);
                System.Diagnostics.Debug.WriteLine("[ReadPage] Successfully loaded initial page content");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ReadPage] Error loading book: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ReadPage] Stack trace: {ex.StackTrace}");
                
                var errorMessage = "Impossible de charger le livre: ";
                if (ex is HttpRequestException)
                {
                    errorMessage += "Erreur de connexion au serveur";
                }
                else if (ex is InvalidDataException)
                {
                    errorMessage += "Format de livre invalide";
                }
                else
                {
                    errorMessage += ex.Message;
                }
                
                await Shell.Current.DisplayAlert("Erreur", errorMessage, "OK");
                await GoBack();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private List<string> ExtractPages(ZipArchive archive)
        {
            var pages = new List<string>();
            var contentFiles = archive.Entries
                .Where(e => e.FullName.EndsWith(".xhtml", StringComparison.OrdinalIgnoreCase) ||
                           e.FullName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.FullName);

            foreach (var entry in contentFiles)
            {
                using var reader = new StreamReader(entry.Open());
                var content = reader.ReadToEnd();
                // Basic HTML cleanup (you might want to enhance this)
                content = System.Text.RegularExpressions.Regex.Replace(content, "<[^>]+>", "");
                content = System.Net.WebUtility.HtmlDecode(content).Trim();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    pages.Add(content);
                }
            }

            return pages;
        }

        [RelayCommand]
        private async Task LoadPageContent(int pageNumber)
        {
            if (IsBusy || pageNumber < 1 || pageNumber > TotalPages) return;

            try
            {
                IsBusy = true;
                
                if (_pages != null && _pages.Count >= pageNumber)
                {
                    PageContent = _pages[pageNumber - 1];
                    CurrentPage = pageNumber;
                    await UpdateProgress(pageNumber);
                }
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
                await _apiService.UpdateLastReadPageAsync(BookId, page);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating progress: {ex.Message}");
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
            if (_epubArchive != null)
            {
                _epubArchive.Dispose();
            }
            await Shell.Current.GoToAsync("..");
        }
    }
} 
