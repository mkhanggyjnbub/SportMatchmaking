using System;
using System.Collections.Generic;

namespace SportMatchmaking.Models;

public partial class Notification
{
    public long NotificationId { get; set; }

    public int UserId { get; set; }

    public string Type { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Body { get; set; }

    public string? DataJson { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public virtual AppUser User { get; set; } = null!;
}
