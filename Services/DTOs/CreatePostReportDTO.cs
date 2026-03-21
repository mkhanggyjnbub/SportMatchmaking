namespace Services.DTOs
{
    public class CreatePostReportDTO
    {
        public long PostId { get; set; }
        public int ReporterUserId { get; set; }
        public byte ReasonCode { get; set; }
        public string? Details { get; set; }
    }
}
