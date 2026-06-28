using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace APLH.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        // Brevo's transactional email API. Plain HTTPS (port 443) — not blocked
        // by hosts (like Render's free tier) that block outbound SMTP ports.
        private const string BrevoApiUrl = "https://api.brevo.com/v3/smtp/email";

        public EmailService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task SendEmailAsync(string toEmail, string name)
        {
            var html = $@"
                <h2>Welcome to APLH, {name}!</h2>
                <p>Your APLH account has been successfully created.</p>
                <p>Now you can login to your account and start learning with us!</p>
                <br>
                <p>Thank you for joining APLH!</p>";

            await SendViaBrevoAsync(toEmail, name, "Welcome to APLH!", html);
        }

        public async Task SendOtpEmailAsync(string toEmail, string otp)
        {
            var html = $@"
                <h2>Password Reset Request</h2>
                <p>You requested to reset your password.</p>
                <h1>{otp}</h1>
                <p>This OTP is valid for 10 minutes.</p>";

            await SendViaBrevoAsync(toEmail, toEmail, "APLH Password Reset OTP", html);
        }

        private async Task SendViaBrevoAsync(string toEmail, string toName, string subject, string htmlContent)
        {
            var apiKey = _configuration["EmailSettings:BrevoApiKey"];
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName = _configuration["EmailSettings:SenderName"] ?? "APLH";

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("EmailSettings:BrevoApiKey is not configured.");
            if (string.IsNullOrWhiteSpace(senderEmail))
                throw new InvalidOperationException("EmailSettings:SenderEmail is not configured.");

            var payload = new
            {
                sender = new { name = senderName, email = senderEmail },
                to = new[] { new { email = toEmail, name = toName } },
                subject,
                htmlContent
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, BrevoApiUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("api-key", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"Brevo API error ({(int)response.StatusCode}): {body}");
            }
        }
    }
}