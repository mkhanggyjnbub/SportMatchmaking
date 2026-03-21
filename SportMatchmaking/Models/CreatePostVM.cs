using System.ComponentModel.DataAnnotations;

namespace SportMatchmaking.Models
{
    public class CreatePostVM
    {
        [Required]
        [Display(Name = "Môn thể thao")]
        public int SportId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Loại trận")]
        public byte MatchType { get; set; } = 1;

        [Required]
        [Display(Name = "Thời gian bắt đầu")]
        public DateTime StartTime { get; set; }

        [Display(Name = "Thời gian kết thúc")]
        public DateTime? EndTime { get; set; }

        [StringLength(200)]
        [Display(Name = "Địa điểm")]
        public string? LocationText { get; set; }

        [StringLength(500)]
        [Display(Name = "Google Maps URL")]
        public string? GoogleMapsUrl { get; set; }

        [StringLength(100)]
        [Display(Name = "Thành phố")]
        public string? City { get; set; }

        [StringLength(100)]
        [Display(Name = "Quận/Huyện")]
        public string? District { get; set; }

        [Range(1, 100)]
        [Display(Name = "Số slot cần tìm")]
        public int SlotsNeeded { get; set; } = 2;

        [Display(Name = "Phí / người")]
        public decimal? FeePerPerson { get; set; }

        [Display(Name = "Gấp")]
        public bool IsUrgent { get; set; }

        [Display(Name = "Skill Min")]
        public byte? SkillMin { get; set; }

        [Display(Name = "Skill Max")]
        public byte? SkillMax { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Hết hạn")]
        public DateTime? ExpiresAt { get; set; }
    }
}