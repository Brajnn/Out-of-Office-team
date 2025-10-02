using System.ComponentModel.DataAnnotations;

namespace Out_of_Office.Models
{
    public class ResetPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        public string Token { get; set; }
    }
}
