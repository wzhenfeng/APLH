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
            await smtp.AuthenticateAsync("zhenfeng.2570@gmail.com", "czsv nbax havh gfvp");
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}