namespace APLH.Models
{
    public class CourseChapter
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int ChapterOrder { get; set; } = 1;
        public string ChapterTitle { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
