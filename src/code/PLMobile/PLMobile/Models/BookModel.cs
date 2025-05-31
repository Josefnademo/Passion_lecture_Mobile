using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PLMobile.Models
{
    public class BookModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("numericId")]
        public int NumericId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("lastReadPage")]
        public int LastReadPage { get; set; }

        public List<TagModel> Tags { get; set; } = new();

        [JsonPropertyName("coverUrl")]
        public string CoverUrl { get; set; }
    }
} 