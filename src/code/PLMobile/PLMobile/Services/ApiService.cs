using PLMobile.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PLMobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:3000";

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };
        }

        public async Task<List<BookModel>> GetBooksAsync(List<int> tagIds = null)
        {
            try
            {
                string url = "/books";
                if (tagIds != null && tagIds.Any())
                {
                    url = $"/books/filter?tagIds={string.Join(",", tagIds)}";
                }

                var response = await _httpClient.GetFromJsonAsync<List<BookModel>>(url);
                return response ?? new List<BookModel>();
            }
            catch (Exception)
            {
                return new List<BookModel>();
            }
        }

        public async Task<List<TagModel>> GetTagsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<TagModel>>("/tags");
                return response ?? new List<TagModel>();
            }
            catch (Exception)
            {
                return new List<TagModel>();
            }
        }

        public async Task<TagModel> CreateTagAsync(string name)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(new { name }),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/tags", content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TagModel>();
        }

        public async Task<byte[]> GetBookContentAsync(int bookId)
        {
            var response = await _httpClient.GetAsync($"/epub/{bookId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<int> GetLastReadPageAsync(int bookId)
        {
            try
            {
                var book = await _httpClient.GetFromJsonAsync<BookModel>($"/books/{bookId}");
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
                JsonSerializer.Serialize(new { page }),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PutAsync($"/books/{bookId}/lastpage", content);
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

            var response = await _httpClient.PostAsync("/upload", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task<byte[]> GetBookEpubAsync(int bookId)
        {
            return await GetBookContentAsync(bookId);
        }
    }
} 