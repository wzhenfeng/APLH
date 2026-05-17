using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using APLH.Services;
using APLH.Models;

namespace APLH.Pages
{
    [Authorize(Roles = "admin")]
    public class AdminModel : PageModel
    {
        private readonly LearningService _service;

        public AdminModel(LearningService service)
        {
            _service = service;
        }

        public List<Course> Courses { get; set; } = new();

        public async Task OnGetAsync()
        {
            Courses = (await _service.GetAllCoursesAsync()).ToList();
            QuizQuestions = (await _service.GetAllQuizQuestionsAsync()).ToList();
        }

        public List<QuizQuestion> QuizQuestions { get; set; } = new();
    }
}