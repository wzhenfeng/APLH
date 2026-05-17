using Microsoft.AspNetCore.Mvc.RazorPages;
using APLH.Services;
using APLH.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace APLH.Pages
{
   [Authorize]
    public class CoursesModel : PageModel
    {
        private readonly LearningService _service;

        public CoursesModel(LearningService service)
        {
            _service = service;
        }

        public List<Course> Courses { get; set; } = new List<Course>();

        public async Task OnGetAsync()
        {
            var courses = await _service.GetAllCoursesAsync();
            Courses = courses as List<Course> ?? new List<Course>(courses);
        }
    }
}