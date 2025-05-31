using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PLMobile.Services;
using PLMobile.Models;
using System.Text;
using System.IO.Compression;
using System.Xml;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace PLMobile.ViewModels
{
    [QueryProperty(nameof(BookId), "BookId")]
    [QueryProperty(nameof(BookTitle), "Title")]
    public partial class ReadPageViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private ZipArchive _epubArchive;
        private List<string> _pages;
        private string _contentOpfPath;
        private Dictionary<string, string> _spineItems;
        private int _currentSpineIndex;

        [ObservableProperty]
        private string _bookId;

        [ObservableProperty]
        private string _bookTitle;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private string _pageContent;

        [ObservableProperty]
        private bool _canGoPrevious;

        [ObservableProperty]
        private bool _canGoNext;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string _progressText;

        private int _currentPage;
        private int _totalPages;
        private float _readingProgress;

        public string CurrentContent
        {
            get => _pageContent;
            set => SetProperty(ref _pageContent, value);
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (SetProperty(ref _currentPage, value))
                {
                    ReadingProgress = _totalPages > 0 ? (float)value / _totalPages : 0;
                    CanGoPrevious = value > 1;
                    CanGoNext = value < _totalPages;
                    UpdateProgressDisplay(value);
                    SaveLastReadPage();
                }
            }
        }

        public float ReadingProgress
        {
            get => _readingProgress;
            set => SetProperty(ref _readingProgress, value);
        }

        public int TotalPages
        {
            get => _totalPages;
            set => SetProperty(ref _totalPages, value);
        }

        public ReadPageViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _pages = new List<string>();
            _spineItems = new Dictionary<string, string>();
            Title = "Lecture";
            CurrentPage = 1;
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

        [RelayCommand]
        private async Task LoadBook()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                System.Diagnostics.Debug.WriteLine($"[ReadPage] Loading book: {BookId}");

                // First verify we can get the book details
                var books = await _apiService.GetBooksAsync();
                var book = books.FirstOrDefault(b => b.Id == BookId);
                if (book == null)
                {
                    throw new Exception("Book not found in library");
                }

                System.Diagnostics.Debug.WriteLine($"[ReadPage] Found book: {book.Title}");

                var epubData = await _apiService.GetBookEpubAsync(BookId);
                System.Diagnostics.Debug.WriteLine($"[ReadPage] Retrieved EPUB data size: {epubData?.Length ?? 0} bytes");

                if (epubData == null || epubData.Length == 0)
                {
                    throw new Exception("No EPUB content received");
                }

                using var stream = new MemoryStream(epubData);
                _epubArchive = new ZipArchive(stream, ZipArchiveMode.Read);

                // Debug: List all entries in the EPUB
                System.Diagnostics.Debug.WriteLine("[ReadPage] EPUB contents:");
                foreach (var entry in _epubArchive.Entries)
                {
                    System.Diagnostics.Debug.WriteLine($"- {entry.FullName}");
                }

                // Find container.xml first
                var containerEntry = _epubArchive.GetEntry("META-INF/container.xml");
                if (containerEntry == null)
                {
                    throw new Exception("Invalid EPUB: Missing container.xml");
                }

                System.Diagnostics.Debug.WriteLine("[ReadPage] Found container.xml");

                // Get content.opf path
                using (var containerReader = new StreamReader(containerEntry.Open()))
                {
                    var containerXml = new XmlDocument();
                    containerXml.Load(containerReader);
                    var nsManager = new XmlNamespaceManager(containerXml.NameTable);
                    nsManager.AddNamespace("n", "urn:oasis:names:tc:opendocument:xmlns:container");
                    var rootfilePath = containerXml.SelectSingleNode("//n:rootfile/@full-path", nsManager)?.Value;
                    if (string.IsNullOrEmpty(rootfilePath))
                    {
                        throw new Exception("Invalid EPUB: Cannot find content.opf path");
                    }
                    _contentOpfPath = rootfilePath;
                    System.Diagnostics.Debug.WriteLine($"[ReadPage] Found content.opf at: {_contentOpfPath}");
                }

                // Process content.opf
                var contentOpfEntry = _epubArchive.GetEntry(_contentOpfPath);
                if (contentOpfEntry == null)
                {
                    throw new Exception("Invalid EPUB: Missing content.opf");
                }

                using (var opfReader = new StreamReader(contentOpfEntry.Open()))
                {
                    var opfXml = new XmlDocument();
                    opfXml.Load(opfReader);
                    var nsManager = new XmlNamespaceManager(opfXml.NameTable);
                    nsManager.AddNamespace("opf", "http://www.idpf.org/2007/opf");

                    // Get spine items
                    _spineItems.Clear();
                    var spineNodes = opfXml.SelectNodes("//opf:spine/opf:itemref", nsManager);
                    var manifestItems = opfXml.SelectNodes("//opf:manifest/opf:item", nsManager);

                    System.Diagnostics.Debug.WriteLine($"[ReadPage] Found {spineNodes?.Count ?? 0} spine items and {manifestItems?.Count ?? 0} manifest items");

                    var idToHref = new Dictionary<string, string>();
                    if (manifestItems != null)
                    {
                        foreach (XmlNode item in manifestItems)
                        {
                            var id = item.Attributes["id"]?.Value;
                            var href = item.Attributes["href"]?.Value;
                            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(href))
                            {
                                idToHref[id] = href;
                                System.Diagnostics.Debug.WriteLine($"[ReadPage] Manifest item: {id} -> {href}");
                            }
                        }
                    }

                    if (spineNodes != null)
                    {
                        foreach (XmlNode itemref in spineNodes)
                        {
                            var idref = itemref.Attributes["idref"]?.Value;
                            if (idToHref.TryGetValue(idref, out string href))
                            {
                                _spineItems[idref] = href;
                                System.Diagnostics.Debug.WriteLine($"[ReadPage] Added spine item: {idref} -> {href}");
                            }
                        }
                    }
                }

                _currentSpineIndex = 0;
                TotalPages = _spineItems.Count;
                System.Diagnostics.Debug.WriteLine($"[ReadPage] Total pages: {TotalPages}");

                // Load last read page
                var lastPage = await _apiService.GetLastReadPageAsync(BookId);
                CurrentPage = lastPage > 0 ? lastPage : 1;
                System.Diagnostics.Debug.WriteLine($"[ReadPage] Starting at page: {CurrentPage}");

                await LoadPageContent(CurrentPage);
                System.Diagnostics.Debug.WriteLine("[ReadPage] Book loaded successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ReadPage] Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ReadPage] Stack trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", "Could not load book: " + ex.Message, "OK");
                await Shell.Current.GoToAsync("..");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadPageContent(int pageNumber)
        {
            if (pageNumber < 1 || pageNumber > TotalPages) return;

            try
            {
                var spineItem = _spineItems.ElementAt(pageNumber - 1);
                var href = spineItem.Value;

                // Resolve relative path
                var contentOpfDir = Path.GetDirectoryName(_contentOpfPath) ?? "";
                var fullPath = Path.Combine(contentOpfDir, href).Replace('\\', '/');

                var entry = _epubArchive.GetEntry(fullPath);
                if (entry == null)
                {
                    throw new Exception($"Cannot find content file: {fullPath}");
                }

                using var reader = new StreamReader(entry.Open());
                var content = await reader.ReadToEndAsync();

                // Clean up the HTML content
                content = CleanHtmlContent(content);

                CurrentPage = pageNumber;
                PageContent = content;
                UpdateProgressDisplay(pageNumber);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ReadPage] Error loading page {pageNumber}: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"Could not load page {pageNumber}: {ex.Message}", "OK");
            }
        }

        private string CleanHtmlContent(string html)
        {
            try
            {
                // Remove scripts and style tags first
                html = Regex.Replace(html, @"<script[^>]*>[\s\S]*?</script>|<style[^>]*>[\s\S]*?</style>", string.Empty);

                // Replace block elements with newlines
                html = Regex.Replace(html, @"</?(p|div|br|h[1-6])[^>]*>", "\n");

                // Remove remaining HTML tags
                html = Regex.Replace(html, "<[^>]+>", string.Empty);

                // Decode HTML entities
                html = System.Net.WebUtility.HtmlDecode(html);

                // Clean up whitespace
                var lines = html.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(line => line.Trim())
                               .Where(line => !string.IsNullOrWhiteSpace(line));

                return string.Join("\n\n", lines);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ReadPage] Error cleaning HTML content: {ex.Message}");
                return html; // Return original content if cleaning fails
            }
        }

        private void UpdateProgressDisplay(int currentPage)
        {
            if (TotalPages > 0)
            {
                ReadingProgress = (float)currentPage / TotalPages;
                ProgressText = $"{currentPage} / {TotalPages}";
            }
            else
            {
                ReadingProgress = 0;
                ProgressText = "0 / 0";
            }
        }

        private async Task SaveLastReadPage()
        {
            try
            {
                await _apiService.UpdateLastReadPageAsync(BookId, CurrentPage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ReadPage] Error saving progress: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                await LoadPageContent(CurrentPage + 1);
            }
        }

        [RelayCommand]
        private async Task PreviousPage()
        {
            if (CurrentPage > 1)
            {
                await LoadPageContent(CurrentPage - 1);
            }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            _epubArchive?.Dispose();
            await Shell.Current.GoToAsync("..");
        }
    }
}

