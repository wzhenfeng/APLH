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

        public string SuccessMessage { get; set; } = "";
        public string ErrorMessage { get; set; } = "";

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Message))
            {
                ErrorMessage = "Please enter your message.";
                return Page();
            }

            var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Student";
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";

            if (!int.TryParse(userIdText, out int userId))
            {
                ErrorMessage = "User session error. Please login again.";
                return Page();
            }

            var chatMessage = new ChatMessage
            {
                UserId = userId,
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

            SuccessMessage = "Your message has been sent successfully.";
            Message = "";

            return Page();
        }
    }
}