using PLMobile.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PLMobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 1000;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
            System.Diagnostics.Debug.WriteLine($"[API] Initialized with base URL: {_httpClient.BaseAddress}");
        }

        public async Task<List<BookModel>> GetBooksAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[API] Starting GetBooksAsync...");

                int retryCount = 0;
                while (retryCount < MaxRetries)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync("/api/books");
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            if (string.IsNullOrEmpty(content))
                            {
                                throw new HttpRequestException("Empty response received");
                            }

                            var books = JsonSerializer.Deserialize<List<BookModel>>(content, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                            if (books != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"[API] Successfully retrieved {books.Count} books");
                                foreach (var book in books)
                                {
                                    // Ensure cover URL is absolute
                                    if (!string.IsNullOrEmpty(book.CoverUrl) && !book.CoverUrl.StartsWith("http"))
                                    {
                                        book.CoverUrl = $"{_httpClient.BaseAddress}{book.CoverUrl.TrimStart('/')}";
                                    }
                                }
                                return books;
                            }
                            return new List<BookModel>();
                        }
                        else
                        {
                            throw new HttpRequestException($"Server returned {response.StatusCode}");
                        }
                    }
                    catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                    {
                        retryCount++;
                        if (retryCount < MaxRetries)
                        {
                            await Task.Delay(RetryDelayMs * retryCount);
                            continue;
                        }
                        throw;
                    }
                }
                throw new HttpRequestException("Max retries exceeded");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Error in GetBooksAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[API] Stack trace: {ex.StackTrace}");
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
            try
            {
                System.Diagnostics.Debug.WriteLine($"[API] Getting book content for book ID: {bookId}");

                int retryCount = 0;
                while (retryCount < MaxRetries)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync($"/api/books/{bookId}/content");

                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsByteArrayAsync();
                            if (content == null || content.Length == 0)
                            {
                                throw new HttpRequestException("Empty book content received");
                            }
                            System.Diagnostics.Debug.WriteLine($"[API] Successfully retrieved book content: {content.Length} bytes");
                            return content;
                        }
                        else
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            System.Diagnostics.Debug.WriteLine($"[API] Error getting book content. Status: {response.StatusCode}, Error: {errorContent}");
                            throw new HttpRequestException($"Failed to get book content. Status: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                    {
                        retryCount++;
                        if (retryCount < MaxRetries)
                        {
                            await Task.Delay(RetryDelayMs * retryCount);
                            continue;
                        }
                        throw;
                    }
                }
                throw new HttpRequestException("Max retries exceeded");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Error in GetBookContentAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[API] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<int> GetLastReadPageAsync(string bookId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/books/{bookId}");
                if (response.IsSuccessStatusCode)
                {
                    var book = await response.Content.ReadFromJsonAsync<BookModel>();
                    return book?.LastReadPage ?? 0;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task UpdateLastReadPageAsync(string bookId, int page)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(new { page = page }),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"/api/books/{bookId}/lastpage", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Error updating last read page: {ex.Message}");
            }
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
            try
            {
                System.Diagnostics.Debug.WriteLine($"[API] Getting EPUB for book ID: {bookId}");
                var content = await GetBookContentAsync(bookId);

                if (content == null || content.Length == 0)
                {
                    throw new Exception("No EPUB content received from server");
                }

                return content;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Error in GetBookEpubAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[API] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[API] Testing connection...");
                int retryCount = 0;
                while (retryCount < MaxRetries)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync("/api/health");
                        var success = response.IsSuccessStatusCode;
                        System.Diagnostics.Debug.WriteLine($"[API] Connection test result: {success}");
                        return success;
                    }
                    catch
                    {
                        retryCount++;
                        if (retryCount < MaxRetries)
                        {
                            await Task.Delay(RetryDelayMs * retryCount);
                            continue;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Connection test failed: {ex.Message}");
                return false;
            }
        }
    }
}