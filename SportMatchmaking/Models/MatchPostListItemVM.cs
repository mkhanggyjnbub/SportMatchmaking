namespace SportMatchmaking.Models
{
    public class MatchPostListItemVM
    {
        public long PostId { get; set; }
        public string Title { get; set; } = "";
        public string SportName { get; set; } = "";
        public string? SportImageUrl { get; set; }
        public string MatchTypeText { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string LocationText { get; set; } = "";
        public string City { get; set; } = "";
        public string District { get; set; } = "";
        public string SkillText { get; set; } = "";
        public decimal? FeePerPerson { get; set; }
        public bool IsUrgent { get; set; }
        public byte Status { get; set; }
        public string StatusText { get; set; } = "";
        public int SlotsNeeded { get; set; }
        public int SlotsRemaining { get; set; }
        public string CreatorName { get; set; } = "";
        public bool CanEdit { get; set; }
        public bool CanCancel { get; set; }
        public bool CanConfirm { get; set; }
        public bool CanComplete { get; set; }
    }
}
