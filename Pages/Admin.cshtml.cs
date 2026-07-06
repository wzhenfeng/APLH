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

        public List<string> UserGrowthLabels { get; set; } = new();
        public List<int> UserGrowthCounts { get; set; } = new();

        public async Task OnGetAsync()
        {
            Courses = (await _service.GetAllCoursesAsync()).ToList();

            var users = (await _service.GetAllUsersAsync()).ToList();

            if (users.Any())
            {
                var minDate = users.Min(u => u.Joined);
                var maxDate = users.Max(u => u.Joined);

                var start = new DateTime(minDate.Year, minDate.Month, 1);
                var end = new DateTime(maxDate.Year, maxDate.Month, 1);

                var counts = users
                    .GroupBy(u => new DateTime(u.Joined.Year, u.Joined.Month, 1))
                    .ToDictionary(g => g.Key, g => g.Count());

                for (var month = start; month <= end; month = month.AddMonths(1))
                {
                    UserGrowthLabels.Add(month.ToString("MMM yyyy"));
                    UserGrowthCounts.Add(counts.TryGetValue(month, out var c) ? c : 0);
                }
            }
        }
    }
}