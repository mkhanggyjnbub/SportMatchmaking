using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SportMatchmaking.Models
{
    public class MatchPostFormVM
    {
        public long? PostId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui long chon mon the thao.")]
        public int SportId { get; set; }

        [Required(ErrorMessage = "Vui long nhap tieu de.")]
        [StringLength(150, ErrorMessage = "Tieu de toi da 150 ky tu.")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Vui long chon loai tran.")]
        public byte MatchType { get; set; } = 4;

        [Required(ErrorMessage = "Vui long chon thoi gian bat dau.")]
        public DateTime StartTime { get; set; } = DateTime.Now.AddHours(2);

        public DateTime? EndTime { get; set; }

        [StringLength(300, ErrorMessage = "Dia diem toi da 300 ky tu.")]
        public string? LocationText { get; set; }

        [StringLength(600, ErrorMessage = "Google Maps URL toi da 600 ky tu.")]
        public string? GoogleMapsUrl { get; set; }

        [StringLength(100, ErrorMessage = "Ten thanh pho toi da 100 ky tu.")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "Ten quan/huyen toi da 100 ky tu.")]
        public string? District { get; set; }

        [Range(1, 10, ErrorMessage = "Skill toi thieu phai trong khoang 1 den 10.")]
        public byte? SkillMin { get; set; }

        [Range(1, 10, ErrorMessage = "Skill toi da phai trong khoang 1 den 10.")]
        public byte? SkillMax { get; set; }

        [Range(1, 50, ErrorMessage = "So slot can them phai tu 1 den 50.")]
        public int SlotsNeeded { get; set; } = 1;

        [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Chi phi phai lon hon hoac bang 0.")]
        public decimal? FeePerPerson { get; set; }

        public bool IsUrgent { get; set; }

        [StringLength(2000, ErrorMessage = "Mo ta toi da 2000 ky tu.")]
        public string? Description { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public string? SelectedSportImageUrl { get; set; }
        public bool IsEdit { get; set; }
        public Dictionary<int, string> SportImageMap { get; set; } = new();
        public IEnumerable<SelectListItem> SportOptions { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> MatchTypeOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
