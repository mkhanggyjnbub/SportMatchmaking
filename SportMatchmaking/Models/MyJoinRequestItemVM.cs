namespace SportMatchmaking.Models
{
    public class MyJoinRequestItemVM
    {
        public long RequestId { get; set; }
        public long PostId { get; set; }
        public string PostTitle { get; set; } = "";
        public int PartySize { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public byte Status { get; set; }
        public bool CanCancel => Status == 1;
    }
}
