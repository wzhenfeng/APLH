using System.ComponentModel.DataAnnotations.Schema;

namespace APLH.Models
{
    // One row per question answered in a single quiz attempt (quiz_scores row).
    public class QuizAnswer
    {
        public int Id { get; set; }

        [Column("quiz_score_id")]
        public int QuizScoreId { get; set; }

        [Column("question_id")]
        public int QuestionId { get; set; }

        [Column("selected_answer")]
        public int SelectedAnswer { get; set; }

        [Column("is_correct")]
        public bool IsCorrect { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    // Joined view used to render the "Review My Answers" screen:
    // the question + options + correct answer + what the student picked.
    public class QuizAnswerReviewItem
    {
        [Column("question_id")]
        public int QuestionId { get; set; }

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
        public int CorrectAnswer { get; set; }

        [Column("selected_answer")]
        public int SelectedAnswer { get; set; }

        [Column("chapter_order")]
        public int ChapterOrder { get; set; } = 1;

        [Column("chapter_title")]
        public string ChapterTitle { get; set; } = string.Empty;

        public List<string> Options => new List<string> { OptionA, OptionB, OptionC, OptionD };
    }
}
