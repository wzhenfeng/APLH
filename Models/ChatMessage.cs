namespace APLH.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public string SenderRole { get; set; } = "";
        public string ReceiverRole { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}