using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PLMobile.Services;
using System.Diagnostics;
using System.Net;

namespace PLMobile.ViewModels
{
    [QueryProperty(nameof(BookId), "bookId")]
    [QueryProperty(nameof(NumericBookId), "numericBookId")]
    public partial class ReadPageViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private string _fullText = "";
        private double _lastScrollPosition = 0;

        [ObservableProperty]
        private string _displayText = "Loading...";

        [ObservableProperty]
        private string _bookTitle;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private double _scrollPosition;

        [ObservableProperty]
        private string _progressText = "0%";

        public string BookId { get; set; }
        public int NumericBookId { get; set; }

        public ReadPageViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        public async Task LoadBook()
        {
            try
            {
                IsLoading = true;
                DisplayText = "Loading book...";

                var response = NumericBookId > 0
                    ? await _apiService.GetBookTextAsync(NumericBookId)
                    : await _apiService.GetBookTextAsync(BookId);

                _fullText = WebUtility.HtmlDecode(response.Text);
                BookTitle = response.Title;

                // Restoring the position
                var positionKey = GetPositionKey();
                _lastScrollPosition = Preferences.Get(positionKey, 0.0);

                DisplayText = _fullText;
                UpdateProgress();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load error: {ex}");
                DisplayText = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateProgress()
        {
            if (string.IsNullOrEmpty(_fullText)) return;

            var progress = _lastScrollPosition * 100;
            ProgressText = $"{progress:F1}%";
        }

        public void SaveReadingPosition(double scrollY, double contentHeight)
        {
            if (contentHeight <= 0) return;

            _lastScrollPosition = scrollY / contentHeight;
            var positionKey = GetPositionKey();
            Preferences.Set(positionKey, _lastScrollPosition);
            UpdateProgress();
        }

        private string GetPositionKey()
        {
            return NumericBookId > 0
                ? $"BookPos_{NumericBookId}"
                : $"BookPos_{BookId}";
        }

        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}