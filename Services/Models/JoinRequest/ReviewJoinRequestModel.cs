using System.ComponentModel.DataAnnotations;

namespace Services.Models.JoinRequest
{
    public class ReviewJoinRequestModel
    {
        [Required]
        public long RequestId { get; set; }

        [Required]
        public long PostId { get; set; }

        [Required]
        public bool IsAccept { get; set; }
    }
}