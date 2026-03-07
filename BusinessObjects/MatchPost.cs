using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class MatchPost
{
    public long PostId { get; set; }

    public int CreatorUserId { get; set; }

    public int SportId { get; set; }

    public string Title { get; set; } = null!;

    public byte MatchType { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? LocationText { get; set; }

    public string? GoogleMapsUrl { get; set; }

    public string? City { get; set; }

    public string? District { get; set; }

    public byte? SkillMin { get; set; }

    public byte? SkillMax { get; set; }

    public int SlotsNeeded { get; set; }

    public decimal? FeePerPerson { get; set; }

    public bool IsUrgent { get; set; }

    public string? Description { get; set; }

    public byte Status { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ChatThread> ChatThreads { get; set; } = new List<ChatThread>();

    public virtual AppUser CreatorUser { get; set; } = null!;

    public virtual ICollection<JoinRequest> JoinRequests { get; set; } = new List<JoinRequest>();

    public virtual ICollection<PostParticipant> PostParticipants { get; set; } = new List<PostParticipant>();

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public virtual Sport Sport { get; set; } = null!;
}
