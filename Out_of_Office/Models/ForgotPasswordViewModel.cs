using System.ComponentModel.DataAnnotations;

namespace Out_of_Office.Models;

public class ForgotPasswordViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";
}