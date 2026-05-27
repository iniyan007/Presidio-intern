using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Phone { get; set; }

    public string? ProfilePicture { get; set; }

    public bool IsActive { get; set; }

    public bool IsEmailVerified { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<MessageThread> MessageThreads { get; set; } = new List<MessageThread>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Packager> PackagerApprovedByNavigations { get; set; } = new List<Packager>();

    public virtual Packager? PackagerUser { get; set; }

    public virtual ICollection<PlatformConfig> PlatformConfigs { get; set; } = new List<PlatformConfig>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<TravelDocument> TravelDocuments { get; set; } = new List<TravelDocument>();
}
