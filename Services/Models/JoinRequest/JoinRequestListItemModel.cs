namespace Services.Models.JoinRequest
{
    public class JoinRequestListItemModel
    {
        public long RequestId { get; set; }
        public long PostId { get; set; }

        public int RequesterUserId { get; set; }
        public string RequesterName { get; set; } = string.Empty;

        public int PartySize { get; set; }
        public string? Message { get; set; }
        public string? GuestNames { get; set; }

        public byte Status { get; set; }
        public string StatusText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? DecidedAt { get; set; }
        public int? DecidedByUserId { get; set; }
    }
}