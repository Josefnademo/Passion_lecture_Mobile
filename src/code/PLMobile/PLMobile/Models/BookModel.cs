using System;
using System.Collections.Generic;

namespace PLMobile.Models
{
    public class BookModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LastReadPage { get; set; }
        public List<string> Tags { get; set; } = new();
        public string CoverUrl { get; set; }
    }
} 