using PLMobile.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PLMobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            System.Diagnostics.Debug.WriteLine($"[API] Initialized with base URL: {_httpClient.BaseAddress}");
        }

        public async Task<List<BookModel>> GetBooksAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[API] Starting GetBooksAsync request...");
                
                // First test the connection
                if (!await TestConnectionAsync())
                {
                    System.Diagnostics.Debug.WriteLine("[API] Connection test failed");
                    throw new HttpRequestException("Cannot connect to API server");
                }

                System.Diagnostics.Debug.WriteLine("[API] Making request to /api/books");
                var books = await _httpClient.GetFromJsonAsync<List<BookModel>>("/api/books");
                
                if (books != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[API] Successfully retrieved {books.Count} books");
                    foreach (var book in books)
                    {
                        System.Diagnostics.Debug.WriteLine($"[API] Book: {book.Id} - {book.Title}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[API] No books returned from API");
                }
                
                return books ?? new List<BookModel>();
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Connection Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[API] Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Connection Error", 
                    $"Could not connect to the server at {_httpClient.BaseAddress}. Please make sure Docker is running and try again.", "OK");
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Unexpected Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[API] Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", 
                    $"An unexpected error occurred while fetching books: {ex.Message}", "OK");
                throw;
            }
        }

        public async Task<List<TagModel>> GetTagsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[API] Starting GetTagsAsync request...");
                
                // First test the connection
                if (!await TestConnectionAsync())
                {
                    System.Diagnostics.Debug.WriteLine("[API] Connection test failed");
                    throw new HttpRequestException("Cannot connect to API server");
                }

                System.Diagnostics.Debug.WriteLine("[API] Making request to /api/tags");
                var tags = await _httpClient.GetFromJsonAsync<List<TagModel>>("/api/tags");
                
                if (tags != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[API] Successfully retrieved {tags.Count} tags");
                    foreach (var tag in tags)
                    {
                        System.Diagnostics.Debug.WriteLine($"[API] Tag: {tag.Id} - {tag.Name}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[API] No tags returned from API");
                }
                
                return tags ?? new List<TagModel>();
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Connection Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[API] Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Connection Error", 
                    $"Could not connect to the server at {_httpClient.BaseAddress}. Please make sure Docker is running and try again.", "OK");
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Unexpected Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[API] Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", 
                    $"An unexpected error occurred while fetching tags: {ex.Message}", "OK");
                throw;
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

        public async Task<byte[]> GetBookContentAsync(string bookId)
        {
            var response = await _httpClient.GetAsync($"/api/books/{bookId}/epub");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<int> GetLastReadPageAsync(string bookId)
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

        public async Task UpdateLastReadPageAsync(string bookId, int page)
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

        public async Task<byte[]> GetBookEpubAsync(string bookId)
        {
            return await GetBookContentAsync(bookId);
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[API] Testing connection...");
                var response = await _httpClient.GetAsync("/api/health");
                var success = response.IsSuccessStatusCode;
                System.Diagnostics.Debug.WriteLine($"[API] Connection test result: {success}");
                if (!success)
                {
                    System.Diagnostics.Debug.WriteLine($"[API] Status code: {response.StatusCode}");
                }
                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Connection test failed: {ex.Message}");
                return false;
            }
        }
    }
} 