using Out_of_Office.Application.Employee;
using Out_of_Office.Application.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Models.AssignProjectViewModel
{
    public class AssignProjectViewModel
    {
        public int ProjectId { get; set; }
        public ProjectDto? Project { get; set; }
        public IEnumerable<EmployeeDto>? Employees { get; set; }
        public int EmployeeId { get; set; }
    }
}
