using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmail(string toEmail, string subject, string body)
    {
        var fromEmail = _config["EmailSettings:Email"];
        var password = _config["EmailSettings:Password"];
        var smtp = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(fromEmail,password),
            EnableSsl = true
        };

        var message = new MailMessage(fromEmail, toEmail, subject, body);

        await smtp.SendMailAsync(message);
    }
}