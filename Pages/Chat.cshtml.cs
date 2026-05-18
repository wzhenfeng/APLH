using APLH.Models;
using APLH.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

[Authorize(Roles = "member")]
public class ChatModel : PageModel;

namespace APLH.Pages
{
    [Authorize]
    public class ChatModel : PageModel
    {
        private readonly LearningService _learningService;

        public ChatModel(LearningService learningService)
        {
            _learningService = learningService;
        }

        [BindProperty]
        public string Message { get; set; } = "";

        public List<ChatMessage> ChatMessages { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return RedirectToPage("/Index");
            }

            ChatMessages = await _learningService.GetChatMessagesByUserIdAsync(userId.Value);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return RedirectToPage("/Index");
            }

            if (string.IsNullOrWhiteSpace(Message))
            {
                ChatMessages = await _learningService.GetChatMessagesByUserIdAsync(userId.Value);
                return Page();
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Student";
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";

            var chatMessage = new ChatMessage
            {
                UserId = userId.Value,
                UserName = userName,
                UserEmail = userEmail,
                SenderRole = "student",
                ReceiverRole = "admin",
                Message = Message
            };

            await _learningService.CreateChatMessageAsync(chatMessage);

            return RedirectToPage("/Chat");
        }

        private int? GetCurrentUserId()
        {
            var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdText, out int userId))
            {
                return userId;
            }

            return null;
        }
    }
}