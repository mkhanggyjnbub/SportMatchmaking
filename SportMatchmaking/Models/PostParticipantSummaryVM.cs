namespace SportMatchmaking.Models
{
    public class PostParticipantSummaryVM
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; } = "";
        public string AvatarUrl { get; set; } = "/images/default-avatar.png";
        public string RoleText { get; set; } = "";
        public int PartySize { get; set; }
        public DateTime JoinedAt { get; set; }
        public byte? SkillLevel { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
    }
}
