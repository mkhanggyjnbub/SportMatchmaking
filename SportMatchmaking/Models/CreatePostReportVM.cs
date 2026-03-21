using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SportMatchmaking.Models
{
    public class CreatePostReportVM
    {
        public long PostId { get; set; }
        public string PostTitle { get; set; } = "";

        [Range(1, 5, ErrorMessage = "Vui lòng chọn lý do report.")]
        public byte ReasonCode { get; set; }

        [StringLength(500, ErrorMessage = "Chi tiết report tối đa 500 ký tự.")]
        public string? Details { get; set; }

        public IEnumerable<SelectListItem> ReasonOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
