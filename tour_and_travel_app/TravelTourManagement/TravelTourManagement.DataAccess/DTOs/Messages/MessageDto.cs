using TravelTourManagement.DataAccess.Enums;

namespace TravelTourManagement.DataAccess.DTOs.Messages;

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid ThreadId { get; set; }
    public Guid SenderId { get; set; }
    public MessageSenderRole SenderRole { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
}
