using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace APLH.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string name)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(
                "APLH",
                _configuration["EmailSettings:Email"]));

            email.To.Add(new MailboxAddress(name, toEmail));

            email.Subject = "Welcome to APLH!";

            email.Body = new TextPart("html")
            {
                Text = $@"
                <h2>Welcome to APLH, {name}!</h2>
                <p>Your APLH account has been successfully created.</p>
                <p>Now you can login to your account and start learning with us!</p>
                <br>
                <p>Thank you for joining APLH!</p>"
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _configuration["EmailSettings:Host"],
                int.Parse(_configuration["EmailSettings:Port"]),
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _configuration["EmailSettings:Email"],
                _configuration["EmailSettings:Password"]);

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }

        public async Task SendOtpEmailAsync(string toEmail, string otp)
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(
                "APLH",
                _configuration["EmailSettings:Email"]));

            email.To.Add(new MailboxAddress(toEmail, toEmail));

            email.Subject = "APLH Password Reset OTP";

            email.Body = new TextPart("html")
            {
                Text = $@"
                <h2>Password Reset Request</h2>
                <p>You requested to reset your password.</p>
                <h1>{otp}</h1>
                <p>This OTP is valid for 10 minutes.</p>"
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _configuration["EmailSettings:Host"],
                int.Parse(_configuration["EmailSettings:Port"]),
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _configuration["EmailSettings:Email"],
                _configuration["EmailSettings:Password"]);

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }
    }
}