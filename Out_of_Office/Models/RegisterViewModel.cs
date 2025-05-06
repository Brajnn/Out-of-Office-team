using System.ComponentModel.DataAnnotations;

namespace Out_of_Office.Models
{
    public class RegisterViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Role { get; set; } // np. "Employee"

        public int? EmployeeId { get; set; } // opcjonalne, jeśli chcesz połączyć

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password), Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
