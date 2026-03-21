namespace SportMatchmaking.Models
{
    public class MatchPostDetailVM
    {
        public long PostId { get; set; }
        public string Title { get; set; } = "";
        public string SportName { get; set; } = "";
        public string? SportImageUrl { get; set; }
        public string MatchTypeText { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string LocationText { get; set; } = "";
        public string? GoogleMapsUrl { get; set; }
        public string City { get; set; } = "";
        public string District { get; set; } = "";
        public string SkillText { get; set; } = "";
        public int SlotsNeeded { get; set; }
        public int SlotsRemaining { get; set; }
        public decimal? FeePerPerson { get; set; }
        public bool IsUrgent { get; set; }
        public string? Description { get; set; }
        public byte Status { get; set; }
        public string StatusText { get; set; } = "";
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ReportCount { get; set; }

        public int CreatorUserId { get; set; }
        public string CreatorDisplayName { get; set; } = "";
        public string CreatorUserName { get; set; } = "";
        public string CreatorAvatarUrl { get; set; } = "/images/default-avatar.png";
        public string? CreatorCity { get; set; }
        public string? CreatorDistrict { get; set; }
        public byte? CreatorSkillLevel { get; set; }

        public bool IsCreator { get; set; }
        public bool AlreadyJoined { get; set; }
        public bool HasPendingJoinRequest { get; set; }
        public bool CanJoin { get; set; }
        public bool CanEdit { get; set; }
        public bool CanCancel { get; set; }
        public bool CanManageRequests { get; set; }
        public bool CanConfirm { get; set; }
        public bool CanComplete { get; set; }
        public bool CanReport { get; set; }
        public bool HasActiveReportByCurrentUser { get; set; }

        public List<PostParticipantSummaryVM> Participants { get; set; } = new();
    }
}
