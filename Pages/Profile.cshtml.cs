using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using APLH.Services;
using APLH.Models;

namespace APLH.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly LearningService _service;

        public ProfileModel(LearningService service)
        {
            _service = service;
        }

        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;

        public List<Course> Course { get; set; } = new();
        public List<QuizScore> QuizScore { get; set; } = new();

        public async Task OnGetAsync()
        {
            UserName = User.FindFirstValue(ClaimTypes.Name) ?? "";
            UserEmail = User.FindFirstValue(ClaimTypes.Email) ?? "";
            UserRole = User.FindFirstValue(ClaimTypes.Role) ?? "";

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            Course = (await _service.GetUserEnrolledCoursesAsync(userId)).ToList();
            QuizScore = (await _service.GetUserQuizScoresAsync(userId)).ToList();
        }
    }
}