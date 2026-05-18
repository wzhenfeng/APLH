using APLH.Models;
using APLH.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

[Authorize(Roles = "admin")]
public class AdminChatModel : PageModel;

namespace APLH.Pages
{
    [Authorize(Roles = "admin")]
    public class AdminChatModel : PageModel
    {
        private readonly LearningService _learningService;

        public AdminChatModel(LearningService learningService)
        {
            _learningService = learningService;
        }

        [BindProperty]
        public int UserId { get; set; }

        [BindProperty]
        public string Message { get; set; } = "";

        public List<ChatMessage> AllMessages { get; set; } = new();

        public async Task OnGetAsync()
        {
            AllMessages = await _learningService.GetAllChatMessagesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (UserId == 0 || string.IsNullOrWhiteSpace(Message))
            {
                return RedirectToPage("/AdminChat");
            }

            var adminName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";
            var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";

            var studentMessages = await _learningService.GetChatMessagesByUserIdAsync(UserId);
            var student = studentMessages.FirstOrDefault();

            if (student == null)
            {
                return RedirectToPage("/AdminChat");
            }

            var reply = new ChatMessage
            {
                UserId = UserId,
                UserName = adminName,
                UserEmail = adminEmail,
                SenderRole = "admin",
                ReceiverRole = "student",
                Message = Message
            };

            await _learningService.CreateChatMessageAsync(reply);

            return RedirectToPage("/AdminChat");
        }
    }
}