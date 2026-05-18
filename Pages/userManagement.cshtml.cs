using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APLH.Models;
using APLH.Services;

namespace APLH.Pages
{
    [Authorize(Roles = "admin")]
    public class UserManagementModel : PageModel
    {
        private readonly LearningService _service;

        public UserManagementModel(LearningService service)
        {
            _service = service;
        }

        public List<User> Users { get; set; } = new();

        public async Task OnGetAsync()
        {
            Users = (await _service.GetAllUsersAsync()).ToList();
        }
    }
}