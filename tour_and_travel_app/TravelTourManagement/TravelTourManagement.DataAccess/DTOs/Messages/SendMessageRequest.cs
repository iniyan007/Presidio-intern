using System.ComponentModel.DataAnnotations;
using TravelTourManagement.DataAccess.Enums;

namespace TravelTourManagement.DataAccess.DTOs.Messages;

public class SendMessageRequest
{
    [Required]
    public Guid ThreadId { get; set; }
    
    [Required]
    public MessageSenderRole SenderRole { get; set; }
    
    [Required]
    public string Body { get; set; } = string.Empty;
}
