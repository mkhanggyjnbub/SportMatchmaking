using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class PostParticipant
{
    public long PostId { get; set; }

    public int UserId { get; set; }

    public byte Role { get; set; }

    public byte Status { get; set; }

    public int PartySize { get; set; }

    public DateTime JoinedAt { get; set; }

    public DateTime? LeftAt { get; set; }

    public virtual MatchPost Post { get; set; } = null!;

    public virtual AppUser User { get; set; } = null!;
}
