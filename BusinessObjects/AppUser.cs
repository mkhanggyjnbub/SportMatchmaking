using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class AppUser
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public string Email { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? DisplayName { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public string? City { get; set; }

    public string? District { get; set; }

    public byte? SkillLevel { get; set; }

    public bool IsBanned { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ICollection<ChatThreadMember> ChatThreadMembers { get; set; } = new List<ChatThreadMember>();

    public virtual ICollection<JoinRequest> JoinRequestDecidedByUsers { get; set; } = new List<JoinRequest>();

    public virtual ICollection<JoinRequest> JoinRequestRequesterUsers { get; set; } = new List<JoinRequest>();

    public virtual ICollection<MatchPost> MatchPosts { get; set; } = new List<MatchPost>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<PostParticipant> PostParticipants { get; set; } = new List<PostParticipant>();

    public virtual ICollection<Report> ReportReporterUsers { get; set; } = new List<Report>();

    public virtual ICollection<Report> ReportTargetUsers { get; set; } = new List<Report>();

    public virtual Role Role { get; set; } = null!;
}
