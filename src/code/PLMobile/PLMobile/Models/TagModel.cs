using System.Text.Json.Serialization;

namespace PLMobile.Models
{
    public class TagModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("booksCount")]
        public int BooksCount { get; set; }
    }
} 