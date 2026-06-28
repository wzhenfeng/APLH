using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace APLH.Services
{
    public class EmailService
    {
        public async Task SendEmailAsync(string toEmail, string name)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("APLH", "zhenfeng.2570@gmail.com"));
            email.To.Add(new MailboxAddress(name, toEmail));
            email.Subject = "Welcome to APLH!";
            email.Body = new TextPart("HTML")
            {
                Text = $@"
                <h2>Welcome to APLH, {name}!</h2>
                <p>Your APLH account has been succesfully been created</p>
                <p>Now you can login to your account and start the learning session with us!</p>
                <br>
                <p>Thank You for joining us APLH!</p>
                "
            };
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync("zhenfeng.2570@gmail.com", "rayq ddbm acff hiql");
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        public async Task SendOtpEmailAsync(string toEmail, string otp)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("APLH", "zhenfeng.2570@gmail.com"));
            email.To.Add(new MailboxAddress(toEmail, toEmail));
            email.Subject = "APLH Password Reset OTP";
            email.Body = new TextPart("HTML")
            {
                Text = $@"
                <h2>Password Reset Request</h2>
                <p>You requested to reset your APLH account password.</p>
                <p>Your One-Time Password (OTP) is:</p>
                <h1 style='letter-spacing:8px; color:#6c63ff;'>{otp}</h1>
                <p>This OTP is valid for <strong>10 minutes</strong>. Do not share it with anyone.</p>
                <p>If you did not request this, please ignore this email.</p>"
            };
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync("zhenfeng.2570@gmail.com", "rayq ddbm acff hiql");
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}