using Microsoft.Maui.Controls;
using PLMobile.Models;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PLMobile.Services
{
    public class ApiService
    {
        public class BookTextResponse
        {
            public string Text { get; set; }
            public string Title { get; set; }
            public int Length { get; set; }
            public string Id { get; set; }
            public int NumericId { get; set; }
        }

        private readonly HttpClient _httpClient;
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 1000;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
            Debug.WriteLine($"[API] Initialized with base URL: {_httpClient.BaseAddress}");
        }

        public async Task<List<BookModel>> GetBooksAsync()
        {
            try
            {
                Debug.WriteLine("[API] Starting GetBooksAsync...");

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
                                Debug.WriteLine($"[API] Successfully retrieved {books.Count} books");
                                foreach (var book in books)
                                {
                                    if (string.IsNullOrEmpty(book.Id))
                                    {
                                        Debug.WriteLine($"[WARNING] Book missing Id: Title={book.Title}, NumericId={book.NumericId}");
                                    }

                                    if (!string.IsNullOrEmpty(book.CoverUrl) && !book.CoverUrl.StartsWith("http"))
                                    {
                                        book.CoverUrl = $"{_httpClient.BaseAddress}{book.CoverUrl.TrimStart('/')}";
                                    }
                                    Debug.WriteLine($"[DEBUG] Response content: {content}");

                                }
                                return books;
                            }
                            return new List<BookModel>();
                        }
                        throw new HttpRequestException($"Server returned {response.StatusCode}");
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
                Debug.WriteLine($"[API] Error in GetBooksAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<BookTextResponse> GetBookTextAsync(string bookId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/books/{bookId}/text");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<BookTextResponse>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[API] Error getting book text: {ex}");
                throw;
            }
        }

        public async Task<BookTextResponse> GetBookTextAsync(int numericId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/books/numeric/{numericId}/text");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<BookTextResponse>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[API] Error getting book text: {ex}");
                throw;
            }
        }

        // Tag-related methods
        public async Task<List<TagModel>> GetTagsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<TagModel>>("/api/tags");
        }

        public async Task<List<TagModel>> GetBookTagsAsync(int numericBookId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<TagModel>>($"/books/{numericBookId}/tags");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[API] Error getting tags for book {numericBookId}: {ex.Message}");
                return new List<TagModel>();
            }
        }

        public async Task<bool> UpdateBookTagsAsync(string bookId, List<string> tagIds)
        {
            try
            {
                var payload = new { tagIds };
                var jsonPayload = JsonSerializer.Serialize(payload);
                Debug.WriteLine($"[API] Sending JSON: {jsonPayload}");

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/books/{bookId}/tags", content);

                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[API] Failed to update tags: {responseText}");
                    return false;
                }

                Debug.WriteLine($"[API] Success: {responseText}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[API] Exception updating tags: {ex.Message}");
                return false;
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
                Debug.WriteLine($"API Connection Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Connection Error",
                    "Could not connect to the server. Please make sure Docker is running and try again.", "OK");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "An unexpected error occurred while creating the tag.", "OK");
                throw;
            }
        }

        public async Task<byte[]> GetBookContentAsync(string bookId)
        {
            try
            {
                Debug.WriteLine($"[API] Getting EPUB content for book ID: {bookId}");
                var response = await _httpClient.GetAsync($"/epub/{bookId}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to get book content. Status: {response.StatusCode}, Details: {errorContent}");
                }

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[API ERROR] GetBookContentAsync failed: {ex}");
                throw;
            }
        }

        public async Task<int> GetLastReadPageAsync(string bookId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/books/{bookId}/lastpage");
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
                    JsonSerializer.Serialize(new { page }),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"/books/{bookId}/lastpage", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[API] Error updating last read page: {ex.Message}");
            }
        }

        public async Task UploadBookAsync(string title, byte[] epubData, byte[] coverImage = null)
        {
            try
            {
                Debug.WriteLine($"[API] Starting upload for book: {title}");

                using var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(epubData), "epub", $"{title}.epub");

                if (coverImage != null)
                {
                    content.Add(new ByteArrayContent(coverImage), "cover", "cover.jpg");
                }

                var response = await _httpClient.PostAsync("/upload", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[API] Upload failed. Status: {response.StatusCode}, Error: {errorContent}");
                    throw new HttpRequestException($"Upload failed: {errorContent}");
                }

                Debug.WriteLine("[API] Upload successful");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[API] Upload error: {ex.Message}");
                throw;
            }
        }

        public async Task<byte[]> GetBookEpubAsync(string bookId)
        {
            try
            {
                Debug.WriteLine($"[API] Fetching EPUB for book ID: {bookId}");

                var response = await _httpClient.GetAsync($"/api/books/{bookId}/epub");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[API ERROR] Status: {response.StatusCode}, Details: {errorContent}");

                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception($"Book {bookId} not found on server");
                    }

                    throw new HttpRequestException($"Failed to get book content. Status: {response.StatusCode}");
                }

                var data = await response.Content.ReadAsByteArrayAsync();
                Debug.WriteLine($"[API] Successfully retrieved EPUB, length: {data.Length} bytes");
                return data;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[API ERROR] GetBookEpubAsync failed: {ex}");
                throw;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                Debug.WriteLine("[API] Testing connection...");
                int retryCount = 0;
                while (retryCount < MaxRetries)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync("/api/health");
                        var success = response.IsSuccessStatusCode;
                        Debug.WriteLine($"[API] Connection test result: {success}");
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
                Debug.WriteLine($"[API] Connection test failed: {ex.Message}");
                return false;
            }
        }
    }
}