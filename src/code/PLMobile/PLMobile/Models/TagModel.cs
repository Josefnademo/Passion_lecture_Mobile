namespace PLMobile.Models
{
    public class TagModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<BookModel> Books { get; set; } = new();
    }
} 