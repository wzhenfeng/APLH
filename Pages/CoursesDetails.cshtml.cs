using Microsoft.AspNetCore.Mvc.RazorPages;
using APLH.Services;
using APLH.Models;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace APLH.Pages
{
    [Authorize]
    public class CourseDetailModel : PageModel
    {
        private readonly LearningService _service;

        public CourseDetailModel(LearningService service)
        {
            _service = service;
        }

        public Course? Course { get; set; }
        public bool IsEnrolled { get; set; }

        public async Task OnGetAsync(int id)
        {
            Course = await _service.GetCourseByIdAsync(id);
            
            if (Course != null && User.Identity?.IsAuthenticated == true)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                IsEnrolled = await _service.IsUserEnrolledAsync(userId, id);
            }
        }

        public string GetCategoryColor()
        {
            return Course?.Category switch
            {
                "Technology" => "#6bcbff",
                "Design" => "#a78bfa",
                "Business" => "#ffd93d",
                "Science" => "#4ade80",
                _ => "#6bcbff"
            };
        }

        public string GetCategoryBg()
        {
            return Course?.Category switch
            {
                "Technology" => "rgba(107,203,255,0.12)",
                "Design" => "rgba(167,139,250,0.12)",
                "Business" => "rgba(255,217,61,0.12)",
                "Science" => "rgba(74,222,128,0.12)",
                _ => "rgba(107,203,255,0.12)"
            };
        }

        public string GetModuleName(int moduleNumber)
        {
            return moduleNumber switch
            {
                1 => "Introduction & Setup",
                2 => "Core Concepts",
                3 => "Hands-on Practice",
                4 => "Advanced Topics",
                5 => "Real Projects",
                6 => "Final Assessment",
                _ => $"Module {moduleNumber}"
            };
        }
    }

    public static class IntExtensions
    {
        public static string ToLocaleString(this int number)
        {
            return number.ToString("N0");
        }
    }
}