using Microsoft.AspNetCore.Identity;
using Out_of_Office.Domain.Entities;


namespace Out_of_Office.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public int? EmployeeId { get; set; }  
        public Employee? Employee { get; set; }
    }
}
