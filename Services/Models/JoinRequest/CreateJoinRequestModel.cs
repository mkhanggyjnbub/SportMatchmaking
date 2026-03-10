using System.ComponentModel.DataAnnotations;

namespace Services.Models.JoinRequest
{
    public class CreateJoinRequestModel
    {
        [Required]
        public long PostId { get; set; }

        [Required]
        [Range(1, 100)]
        public int PartySize { get; set; }

        [StringLength(500)]
        public string? Message { get; set; }

        [StringLength(500)]
        public string? GuestNames { get; set; }
    }
}