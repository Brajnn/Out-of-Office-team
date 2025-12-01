using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Domain.Entities
{
    public class Project
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string ProjectType { get; set; }
        public DateTime StartDate { get; set; }  

        public DateTime? EndDate { get; set; }
        public int ProjectManagerID { get; set; }  
        public Employee ProjectManager { get; set; } 

        public string? Comment { get; set; }
        public ProjectStatus Status { get; set; }
        public ICollection<EmployeeProject> EmployeeProjects { get; set; }
        public Project()
        {
            EmployeeProjects = new List<EmployeeProject>(); 
        }
    }


    public enum ProjectStatus
    {
        Active,
        Inactive
    }
}
