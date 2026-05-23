using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APLH.Models;
using APLH.Services;

namespace APLH.Pages
{
    [Authorize(Roles = "admin")]
    public class CourseManagementModel : PageModel
    {
        private readonly LearningService _service;

        public CourseManagementModel(LearningService service)
        {
            _service = service;
        }

        public List<Course> Courses { get; set; } = new();

        public async Task OnGetAsync()
        {
            Courses = (await _service.GetAllCoursesAsync()).ToList();
        }
    }
}