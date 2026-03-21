namespace SportMatchmaking.Models
{
    public class PostJoinRequestItemVM
    {
        public long RequestId { get; set; }
        public int RequesterUserId { get; set; }
        public string RequesterName { get; set; } = "";
        public byte? RequesterSkillLevel { get; set; }
        public int PartySize { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public byte Status { get; set; }

        public bool CanApprove => Status == 1;
    }
}
