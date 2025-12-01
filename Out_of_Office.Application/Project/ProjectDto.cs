using Out_of_Office.Application.Employee;
using Out_of_Office.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Project
{
    public class ProjectDto
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string ProjectType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ProjectManagerFullName { get; set; } 
        public string? Comment { get; set; }
        public ProjectStatus Status { get; set; }
        public List<EmployeeDto> Employees { get; set; }
    }
}
