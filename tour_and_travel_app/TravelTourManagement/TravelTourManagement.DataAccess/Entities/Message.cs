using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class Message
{
    public Guid Id { get; set; }

    public Guid ThreadId { get; set; }

    public Guid SenderId { get; set; }

    public string Body { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime SentAt { get; set; }

    public virtual User Sender { get; set; } = null!;

    public virtual MessageThread Thread { get; set; } = null!;
}
