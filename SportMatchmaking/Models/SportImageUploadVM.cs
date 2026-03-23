using System.ComponentModel.DataAnnotations;

namespace SportMatchmaking.Models
{
    public class SportImageUploadVM
    {
        public int ImageId { get; set; }

        public string? CurrentImageUrl { get; set; }

        [Display(Name = "Ảnh môn thể thao")]
        public IFormFile? ImageFile { get; set; }
    }
}
