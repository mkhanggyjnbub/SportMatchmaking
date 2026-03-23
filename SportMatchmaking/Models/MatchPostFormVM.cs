using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SportMatchmaking.Models
{
    public class MatchPostFormVM
    {
        public long? PostId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn môn thể thao.")]
        public int SportId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
        [StringLength(150, ErrorMessage = "Tiêu đề tối đa 150 ký tự.")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng chọn loại trận.")]
        public byte MatchType { get; set; } = 4;

        [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu.")]
        public DateTime StartTime { get; set; } = DateTime.Now.AddHours(2);

        [Required(ErrorMessage = "Vui lòng chọn thời gian kết thúc trận.")]
        public DateTime? EndTime { get; set; }

        [StringLength(300, ErrorMessage = "Địa điểm tối đa 300 ký tự.")]
        public string? LocationText { get; set; }

        [StringLength(600, ErrorMessage = "Google Maps URL tối đa 600 ký tự.")]
        public string? GoogleMapsUrl { get; set; }

        [StringLength(100, ErrorMessage = "Tên thành phố tối đa 100 ký tự.")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "Tên quận/huyện tối đa 100 ký tự.")]
        public string? District { get; set; }

        [Range(1, 10, ErrorMessage = "Skill tối thiểu phải nằm trong khoảng 1 đến 10.")]
        public byte? SkillMin { get; set; }

        [Range(1, 10, ErrorMessage = "Skill tối đa phải nằm trong khoảng 1 đến 10.")]
        public byte? SkillMax { get; set; }

        [Range(1, 50, ErrorMessage = "Số slot cần thêm phải từ 1 đến 50.")]
        public int SlotsNeeded { get; set; } = 1;

        [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Chi phí phải lớn hơn hoặc bằng 0.")]
        public decimal? FeePerPerson { get; set; }

        public bool IsUrgent { get; set; }

        [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hạn chốt.")]
        public DateTime? ExpiresAt { get; set; }

        public string? SelectedSportImageUrl { get; set; }
        public bool IsEdit { get; set; }
        public Dictionary<int, string> SportImageMap { get; set; } = new();
        public IEnumerable<SelectListItem> SportOptions { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> MatchTypeOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
