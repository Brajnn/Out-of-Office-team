using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Out_of_Office.Models
{
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
