using System.Text;

namespace APLH.Services
{
    public class WhatsAppService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public WhatsAppService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task SendWhatsAppMessageAsync(string message)
        {
            var accountSid = _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];
            var fromNumber = _configuration["Twilio:FromWhatsAppNumber"];
            var toNumber = _configuration["Twilio:ToWhatsAppNumber"];

            if (string.IsNullOrWhiteSpace(accountSid) ||
                string.IsNullOrWhiteSpace(authToken) ||
                string.IsNullOrWhiteSpace(fromNumber) ||
                string.IsNullOrWhiteSpace(toNumber))
            {
                return;
            }

            var url = $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json";

            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("From", fromNumber),
                new KeyValuePair<string, string>("To", toNumber),
                new KeyValuePair<string, string>("Body", message)
            });

            var authBytes = Encoding.ASCII.GetBytes($"{accountSid}:{authToken}");
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(authBytes)
                );

            await _httpClient.PostAsync(url, data);
        }
    }
}