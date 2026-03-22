using System.ComponentModel.DataAnnotations;

namespace SportMatchmaking.Models
{
    public class ResetPasswordVM
    {
        [Required]
        public string ResetToken { get; set; } = null!;

        [Required(ErrorMessage = "New password is required")]
        [RegularExpression(
            @"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{6,}$",
            ErrorMessage = "Password must be at least 6 characters long and contain at least one uppercase letter, one number, and one special character.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Password does not match")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
