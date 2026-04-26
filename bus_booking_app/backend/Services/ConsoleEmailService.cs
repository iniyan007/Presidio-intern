namespace backend.Services
{
    public class ConsoleEmailService : IEmailService
    {
        public Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
        {
            Console.WriteLine($"\n================ EMAIL DISPATCHED ================");
            Console.WriteLine($"TO: {toEmail}");
            Console.WriteLine($"SUBJECT: {subject}");
            Console.WriteLine($"--------------------------------------------------");
            Console.WriteLine($"{body}");
            Console.WriteLine($"==================================================\n");
            return Task.CompletedTask;
        }
    }
}
