namespace APLH.Models
{
    public class QuizScore
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public decimal Percentage { get; set; }
        public DateTime QuizDate { get; set; }
    }
}