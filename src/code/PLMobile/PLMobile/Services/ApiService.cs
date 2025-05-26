using PLMobile.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PLMobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            // For Android Emulator, use 10.0.2.2
            // For Windows/iOS debugging, use localhost
            string baseUrl = DeviceInfo.Platform == DevicePlatform.Android 
                ? "http://10.0.2.2:3000"
                : "http://localhost:3000";

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Connection Error: {ex.Message}");
                return false;
            }
        }

        public async Task<List<BookModel>> GetBooksAsync(List<int> tagIds = null)
        {
            try
            {
                // Health check
                var healthResponse = await _httpClient.GetAsync("/health");
                if (!healthResponse.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("API server is not healthy");
                }

                string url = "/api/books";
                if (tagIds != null && tagIds.Any())
                {
                    url = $"/api/books/filter?tagIds={string.Join(",", tagIds)}";
                }

                var response = await _httpClient.GetFromJsonAsync<List<BookModel>>(url);
                return response ?? new List<BookModel>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Connection Error", 
                    "Could not connect to the server. Please make sure Docker is running and try again.", "OK");
                return new List<BookModel>();
            }
        }

        public async Task<List<TagModel>> GetTagsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<TagModel>>("/api/tags");
                return response ?? new List<TagModel>();
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Connection Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Connection Error", 
                    "Could not connect to the server. Please make sure Docker is running and try again.", "OK");
                return new List<TagModel>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", 
                    "An unexpected error occurred while fetching tags.", "OK");
                return new List<TagModel>();
            }
        }

        public async Task<TagModel> CreateTagAsync(string name)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(new { name }),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("/api/tags", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<TagModel>();
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Connection Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Connection Error", 
                    "Could not connect to the server. Please make sure Docker is running and try again.", "OK");
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", 
                    "An unexpected error occurred while creating the tag.", "OK");
                throw;
            }
        }

        public async Task<byte[]> GetBookContentAsync(int bookId)
        {
            var response = await _httpClient.GetAsync($"/api/books/{bookId}/epub");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<int> GetLastReadPageAsync(int bookId)
        {
            try
            {
                var book = await _httpClient.GetFromJsonAsync<BookModel>($"/api/books/{bookId}");
                return book?.LastReadPage ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task UpdateLastReadPageAsync(int bookId, int page)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(new { lastReadPage = page }),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PutAsync($"/api/books/{bookId}/lastpage", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task UploadBookAsync(string title, byte[] epubData, byte[] coverImage = null)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(epubData), "epub", $"{title}.epub");
            
            if (coverImage != null)
            {
                content.Add(new ByteArrayContent(coverImage), "cover", "cover.jpg");
            }

            content.Add(new StringContent(title), "title");

            var response = await _httpClient.PostAsync("/api/books/upload", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task<byte[]> GetBookEpubAsync(int bookId)
        {
            return await GetBookContentAsync(bookId);
        }
    }
} 