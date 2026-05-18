using APLH.Models;
using APLH.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace APLH.Pages
{
    [Authorize]
    public class ChatModel : PageModel
    {
        private readonly LearningService _learningService;
        private readonly WhatsAppService _whatsAppService;

        public ChatModel(LearningService learningService, WhatsAppService whatsAppService)
        {
            _learningService = learningService;
            _whatsAppService = whatsAppService;
        }

        [BindProperty]
        public string Message { get; set; } = "";

        public List<ChatMessage> ChatMessages { get; set; } = new();

        public string SuccessMessage { get; set; } = "";
        public string ErrorMessage { get; set; } = "";

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
                ErrorMessage = "User session error. Please login again.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Message))
            {
                ErrorMessage = "Please enter your message.";
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
                Message = Message
            };

            await _learningService.CreateChatMessageAsync(chatMessage);

            var whatsappText =
                $"New APLH Chat Message\n\n" +
                $"From: {userName}\n" +
                $"Email: {userEmail}\n\n" +
                $"Message:\n{Message}";

            await _whatsAppService.SendWhatsAppMessageAsync(whatsappText);

            SuccessMessage = "Message sent.";

            Message = "";

            ChatMessages = await _learningService.GetChatMessagesByUserIdAsync(userId.Value);

            return Page();
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