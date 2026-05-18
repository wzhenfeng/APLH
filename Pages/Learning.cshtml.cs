using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APLH.Models;
using APLH.Services;

namespace APLH.Pages
{
    public class LearningModel : PageModel
    {
        private readonly LearningService _service;

        public LearningModel(LearningService service)
        {
            _service = service;
        }

        public Course Course { get; set; } = new();

        public List<CourseMaterial> Materials { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int courseId)
        {
            Course = await _service.GetCourseByIdAsync(courseId);

            if (Course == null)
            {
                return RedirectToPage("/Courses");
            }

            Materials = (await _service.GetCourseMaterialsAsync(courseId)).ToList();

            return Page();
        }
    }
}