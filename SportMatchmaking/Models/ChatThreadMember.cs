using System;
using System.Collections.Generic;

namespace SportMatchmaking.Models;

public partial class ChatThreadMember
{
    public long ThreadId { get; set; }

    public int UserId { get; set; }

    public DateTime JoinedAt { get; set; }

    public bool IsMuted { get; set; }

    public virtual ChatThread Thread { get; set; } = null!;

    public virtual AppUser User { get; set; } = null!;
}
