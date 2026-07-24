using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APLH.Models;
using APLH.Services;
using System.Security.Claims;

namespace APLH.Pages
{
    [Authorize]
    public class PaymentModel : PageModel
    {
        private readonly LearningService _service;

        public PaymentModel(LearningService service)
        {
            _service = service;
        }

        public Course Course { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int courseId)
        {
            var course = await _service.GetCourseByIdAsync(courseId);

            if (course == null)
            {
                return RedirectToPage("/Courses");
            }

            // Free courses never need the payment page - send straight back.
            if (course.Price <= 0)
            {
                return RedirectToPage("/CoursesDetails", new { id = courseId });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var alreadyEnrolled = await _service.IsUserEnrolledAsync(userId, courseId);

            if (alreadyEnrolled)
            {
                return RedirectToPage("/CoursesDetails", new { id = courseId });
            }

            Course = course;
            return Page();
        }
    }
}
