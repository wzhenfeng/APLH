using System.ComponentModel.DataAnnotations.Schema;

namespace APLH.Models
{
    public class QuizQuestion
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        
        [Column("option_a")]
        public string OptionA { get; set; } = string.Empty;
        
        [Column("option_b")]
        public string OptionB { get; set; } = string.Empty;
        
        [Column("option_c")]
        public string OptionC { get; set; } = string.Empty;
        
        [Column("option_d")]
        public string OptionD { get; set; } = string.Empty;
        
        [Column("correct_answer")]
        public int CorrectAnswer { get; set; } // 0=A,1=B,2=C,3=D
        
        [Column("course_id")]
        public int? CourseId { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        
        public List<string> Options => new List<string> { OptionA, OptionB, OptionC, OptionD };
    }
}