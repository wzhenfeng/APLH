using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APLH.Models;
using APLH.Services;

namespace APLH.Pages
{
    [Authorize(Roles = "admin")]
    public class ActivityLogsModel : PageModel
    {
        private readonly LearningService _service;

        public ActivityLogsModel(LearningService service)
        {
            _service = service;
        }

        public List<ActivityLog> Logs { get; set; } = new();

        public async Task OnGetAsync()
        {
            Logs = (await _service.GetAllActivityLogsAsync()).ToList();
        }
    }
}