using System.ComponentModel.DataAnnotations;

namespace Out_of_Office.Models
{
    public class EnableAuthenticatorViewModel
    {
        public string Key { get; set; }
        [Required(ErrorMessage = "CodeRequiredError")]
        [Display(Name = "CodeLabel")]
        public string Code { get; set; }
    }
}
