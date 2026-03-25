using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class JoinRequest
{
    public long RequestId { get; set; }

    public long PostId { get; set; }

    public int RequesterUserId { get; set; }

    public byte SkillLevel { get; set; }

    public int PartySize { get; set; }

    public string? Message { get; set; }

    public string? GuestNames { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DecidedAt { get; set; }

    public int? DecidedByUserId { get; set; }

    public virtual AppUser? DecidedByUser { get; set; }

    public virtual MatchPost Post { get; set; } = null!;

    public virtual AppUser RequesterUser { get; set; } = null!;
}
