using Microsoft.AspNetCore.Mvc.RazorPages;
using APLH.Services;
using APLH.Models;

namespace APLH.Pages
{
    public class TestQuizModel : PageModel
    {
        private readonly LearningService _service;

        public TestQuizModel(LearningService service)
        {
            _service = service;
        }

        public int QuestionCount { get; set; }
        public IEnumerable<QuizQuestion>? Questions { get; set; }

        public async Task OnGetAsync()
        {
            Questions = await _service.GetAllQuizQuestionsAsync();
            QuestionCount = Questions.Count();
        }
    }
}
