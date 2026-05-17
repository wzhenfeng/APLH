namespace APLH.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Activity { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}