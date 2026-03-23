namespace Services.DTOs
{
    public class MatchPostSearchDTO
    {
        public string? Keyword { get; set; }
        public int? SportId { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public DateTime? StartFrom { get; set; }
        public DateTime? StartTo { get; set; }
        public byte? SkillLevel { get; set; }
        public byte? Status { get; set; }
        public bool? IsUrgent { get; set; }
        public byte? MatchType { get; set; }
        public int? CreatorUserId { get; set; }
        public int? ViewerUserId { get; set; }
        public bool ExploreOnlyActivePosts { get; set; }
    }
}
