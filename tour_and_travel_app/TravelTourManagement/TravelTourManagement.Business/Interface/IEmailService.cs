using System.Threading;
using System.Threading.Tasks;

namespace TravelTourManagement.Business.Interface;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
}
