using System.ComponentModel.DataAnnotations;

namespace SportMatchmaking.Models
{
    public class RegisterVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "UserName must be from 3 to 20 characters.")]
        [RegularExpression(@"^[A-Za-z][A-Za-z0-9_]*$", ErrorMessage = "UserName must start with a letter and can only contain letters, numbers, and underscore (_) without spaces.")]
        public string UserName { get; set; } = null!;

        [Required]
        [RegularExpression(
    @"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{6,}$",
    ErrorMessage = "Password must be at least 6 characters long and contain at least one uppercase letter, one number, and one special character."
)]
        public string Password { get; set; } = null!;

        [Required]
        [Compare("Password", ErrorMessage = "Password not match")]
        public string ConfirmPassword { get; set; } = null!;

        [RegularExpression(@"^(0[3|5|7|8|9])[0-9]{8}$",
         ErrorMessage = "Phone number invalid")]
        public string PhoneNumber { get; set; } = "";

        public string DisplayName { get; set; } = "";

        public string? Bio { get; set; }

        public string City { get; set; } = "";

        public string District { get; set; } = "";

    }
}
