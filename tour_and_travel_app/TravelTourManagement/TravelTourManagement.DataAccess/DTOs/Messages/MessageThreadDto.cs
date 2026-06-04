namespace TravelTourManagement.DataAccess.DTOs.Messages;

public class MessageThreadDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PackagerId { get; set; }
    public Guid? PackageId { get; set; }
    
    public string UserName { get; set; } = string.Empty;
    public string UserProfilePicture { get; set; } = string.Empty;
    
    public string PackagerName { get; set; } = string.Empty;
    public string PackagerProfilePicture { get; set; } = string.Empty;

    public string? PackageTitle { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    
    public MessageDto? LastMessage { get; set; }
    public int UnreadCount { get; set; }
}
