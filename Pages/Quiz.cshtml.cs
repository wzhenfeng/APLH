using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APLH.Services;

namespace APLH.Pages
{
    [Authorize]
    public class QuizModel : PageModel
    {
        private readonly LearningService _service;

        public QuizModel(LearningService service)
        {
            _service = service;
        }

        // Total minutes the course creator set for this course; the quiz splits
        // this evenly across however many chapters the quiz has.
        public int CourseDurationMinutes { get; set; }

        public async Task OnGetAsync(int? courseId)
        {
            if (courseId.HasValue)
            {
                var course = await _service.GetCourseByIdAsync(courseId.Value);
                if (course != null)
                {
                    CourseDurationMinutes = course.Duration;
                }
            }
        }
    }
}