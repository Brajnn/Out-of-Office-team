using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.User
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int EmployeeId { get; set; }
        public string Role { get; set; }
    }
}
