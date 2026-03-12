using System.ComponentModel.DataAnnotations;

namespace SportMatchmaking.Models
{
    public class ChangePasswordVM
    {
        [Required(ErrorMessage = "Old password is required")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; } = "";

        [Required(ErrorMessage = "New password is required")]
        [RegularExpression(
    @"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{6,}$",
    ErrorMessage = "Password must be at least 6 characters long and contain at least one uppercase letter, one number, and one special character."
)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Confirm password does not match")]
        public string ConfirmPassword { get; set; } = "";
    }
}
