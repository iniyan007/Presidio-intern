using System.Net;
using System.Net.Mail;

namespace backend.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
        {
            var emailSettings = _config.GetSection("EmailSettings");
            var senderEmail = emailSettings["SenderEmail"];
            var senderPassword = emailSettings["SenderPassword"];
            var senderName = emailSettings["SenderName"];
            var smtpServer = emailSettings["SmtpServer"];
            var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");

            if (string.IsNullOrEmpty(senderEmail) || senderEmail == "YOUR_GMAIL_ID_HERE@gmail.com")
            {
                Console.WriteLine("SMTP Credentials not configured! Falling back to console log.");
                Console.WriteLine($"\n================ EMAIL DISPATCHED ================");
                Console.WriteLine($"TO: {toEmail}");
                Console.WriteLine($"SUBJECT: {subject}");
                Console.WriteLine($"--------------------------------------------------");
                Console.WriteLine($"{body}");
                Console.WriteLine($"==================================================\n");
                return;
            }

            var message = new MailMessage();
            message.From = new MailAddress(senderEmail, senderName);
            message.To.Add(new MailAddress(toEmail));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isHtml;

            using var client = new SmtpClient(smtpServer, smtpPort);
            client.Credentials = new NetworkCredential(senderEmail, senderPassword);
            client.EnableSsl = true;

            try
            {
                await client.SendMailAsync(message);
                Console.WriteLine($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email to {toEmail}: {ex.Message}");
            }
        }
    }
}
