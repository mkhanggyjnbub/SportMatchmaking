using System.ComponentModel.DataAnnotations;

namespace SportMatchmaking.Models
{
    public class CreateJoinRequestVM
    {
        [Required(ErrorMessage = "PostId là bắt buộc")]
        public long PostId { get; set; }

        [Range(1, 10, ErrorMessage = "Kỹ năng phải từ 1 đến 10")]
        public byte? SkillLevel { get; set; }

        [Required(ErrorMessage = "PartySize là bắt buộc")]
        [Range(1, 30, ErrorMessage = "PartySize phải từ 1 đến 30")]
        public int PartySize { get; set; } = 1;

        [Required(ErrorMessage = "Message là bắt buộc")]
        [StringLength(500, ErrorMessage = "Message tối đa 500 ký tự")]
        public string? Message { get; set; }

        [StringLength(300, ErrorMessage = "GuestNames tối đa 300 ký tự")]
        public string? GuestNames { get; set; }
    }
}
