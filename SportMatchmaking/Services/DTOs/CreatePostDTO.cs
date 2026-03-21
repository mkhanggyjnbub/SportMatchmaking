namespace Services.DTOs
{
    public class CreatePostDTO
    {
        public int SportId { get; set; }
        public string Title { get; set; } = string.Empty;
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
        public DateTime? ExpiresAt { get; set; }
    }
}
