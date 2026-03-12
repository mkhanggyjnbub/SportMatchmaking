using System.ComponentModel.DataAnnotations;

namespace SportMatchmaking.Models
{
    public class ProfileVM
    {
        public int UserId { get; set; }

        public string? Email { get; set; } = null!;

        public string? UserName { get; set; } = null!;

        [StringLength(50, ErrorMessage = "Display name must not exceed 50 characters.")]
        public string DisplayName { get; set; } = "";

        public string? AvatarUrl { get; set; }

        public IFormFile? AvatarFile { get; set; }

        [StringLength(500, ErrorMessage = "Bio must not exceed 500 characters.")]
        public string? Bio { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; } = "";

        [StringLength(100, ErrorMessage = "City must not exceed 100 characters.")]
        public string City { get; set; } = "";

        [StringLength(100, ErrorMessage = "District must not exceed 100 characters.")]
        public string District { get; set; } = "";

        [Range(1, 10, ErrorMessage = "Skill level must be between 1 and 10.")]
        public byte SkillLevel { get; set; }
    }
}
