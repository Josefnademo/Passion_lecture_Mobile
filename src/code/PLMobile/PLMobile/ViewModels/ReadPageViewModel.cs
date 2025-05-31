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
        private int _lastPosition = 0;
        private const int ChunkSize = 10000;
        private double _scrollPosition;

        [ObservableProperty]
        private string _displayText = "Loading...";

        public double ScrollPosition
        {
            get => _scrollPosition;
            set => SetProperty(ref _scrollPosition, value);
        }

        [ObservableProperty]
        private string _progressText = "0%";

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _bookTitle;

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
                DisplayText = "Loading book content...";

                ApiService.BookTextResponse response;
                if (NumericBookId > 0)
                {
                    response = await _apiService.GetBookTextAsync(NumericBookId);
                }
                else
                {
                    response = await _apiService.GetBookTextAsync(BookId);
                }

                _fullText = WebUtility.HtmlDecode(response.Text); // Decode HTML entities
                BookTitle = response.Title;

                // Restore position
                var positionKey = NumericBookId > 0 ? $"Pos_{NumericBookId}" : $"Pos_{BookId}";
                _lastPosition = Preferences.Get(positionKey, 0);

                UpdateDisplayText();
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

        private void UpdateDisplayText()
        {
            try
            {
                if (string.IsNullOrEmpty(_fullText))
                {
                    DisplayText = "No text available";
                    return;
                }

                _lastPosition = Math.Clamp(_lastPosition, 0, _fullText.Length - 1);
                var displayLength = Math.Min(ChunkSize, _fullText.Length - _lastPosition);
                DisplayText = _fullText.Substring(_lastPosition, displayLength);

                var progress = (double)_lastPosition / _fullText.Length;
                ProgressText = $"{progress:P0}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Display error: {ex}");
                DisplayText = "Error displaying text";
            }
        }

        [ObservableProperty]
        private bool _hasMoreText = true;

        [RelayCommand]
        private void LoadMore()
        {
            _lastPosition += ChunkSize;
            if (_lastPosition >= _fullText.Length)
            {
                _lastPosition = _fullText.Length - 1;
                HasMoreText = false;
            }
            UpdateDisplayText();

            // Save position
            var positionKey = NumericBookId > 0 ? $"Pos_{NumericBookId}" : $"Pos_{BookId}";
            Preferences.Set(positionKey, _lastPosition);
        }

        public void SavePosition(double position)
        {
            try
            {
                if (_fullText.Length == 0) return;

                var newPosition = (int)(position * _fullText.Length);
                _lastPosition = Math.Clamp(newPosition, 0, _fullText.Length - 1);

                var positionKey = NumericBookId > 0
                    ? $"Pos_{NumericBookId}"
                    : $"Pos_{BookId}";
                Preferences.Set(positionKey, _lastPosition);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] SavePosition failed: {ex}");
            }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            try
            {
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] GoBack failed: {ex}");
            }
        }
    }
}