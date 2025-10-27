using System.ComponentModel.DataAnnotations;

namespace Out_of_Office.Models
{
    public class LoginWithRecoveryCodeViewModel
    {
        [Required]
        [Display(Name = "RecoveryCode")]
        public string RecoveryCode { get; set; } = string.Empty;
    }
}
