using System.Net.Http.Json;
using System.Text.Json;

namespace PassionLecture.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:3000"; // Update with your actual API URL

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        // Book related methods
        public async Task<List<Book>> GetBooksAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Book>>("/books") ?? new List<Book>();
        }

        public async Task<List<Book>> GetBooksByTagAsync(string[] tagIds)
        {
            string tagIdsParam = string.Join(",", tagIds);
            return await _httpClient.GetFromJsonAsync<List<Book>>($"/books/filter?tagIds={tagIdsParam}") ?? new List<Book>();
        }

        public async Task<byte[]> GetBookEpubAsync(string bookId)
        {
            return await _httpClient.GetByteArrayAsync($"/epub/{bookId}");
        }

        public async Task<byte[]> GetBookCoverAsync(string bookId)
        {
            return await _httpClient.GetByteArrayAsync($"/books/{bookId}/cover");
        }

        public async Task UpdateLastReadPageAsync(string bookId, int page)
        {
            await _httpClient.PutAsJsonAsync($"/books/{bookId}/lastpage", new { page });
        }

        // Tag related methods
        public async Task<List<Tag>> GetTagsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Tag>>("/tags") ?? new List<Tag>();
        }

        public async Task<Tag> CreateTagAsync(string name)
        {
            var response = await _httpClient.PostAsJsonAsync("/tags", new { name });
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Tag>(content) ?? throw new Exception("Failed to create tag");
        }

        public async Task<List<Tag>> GetBookTagsAsync(string bookId)
        {
            return await _httpClient.GetFromJsonAsync<List<Tag>>($"/books/{bookId}/tags") ?? new List<Tag>();
        }

        public async Task AssociateTagsWithBookAsync(string bookId, string[] tagIds)
        {
            await _httpClient.PostAsJsonAsync($"/books/{bookId}/tags", new { tagIds });
        }

        // Book upload method
        public async Task UploadBookAsync(string title, byte[] epubData, byte[] coverData)
        {
            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(epubData), "epub", $"{title}.epub");
            if (coverData != null)
            {
                content.Add(new ByteArrayContent(coverData), "cover", "cover.jpg");
            }
            await _httpClient.PostAsync("/upload", content);
        }
    }

    public class Book
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int LastReadPage { get; set; }
        public List<Tag> Tags { get; set; } = new List<Tag>();
    }

    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
} 