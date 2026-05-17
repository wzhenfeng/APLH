namespace APLH.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public string Emoji { get; set; } = string.Empty;
        public int Enrolled { get; set; }
        public decimal Rating { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}