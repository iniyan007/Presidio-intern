using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TravelTourManagement.Business.Interface;

namespace TravelTourManagement.Business.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        // Mock email sending by logging it to the console
        _logger.LogInformation("\n========== MOCK EMAIL SENT ==========\nTo: {ToEmail}\nSubject: {Subject}\nBody:\n{Body}\n=====================================\n", 
            toEmail, subject, body);
            
        return Task.CompletedTask;
    }
}
