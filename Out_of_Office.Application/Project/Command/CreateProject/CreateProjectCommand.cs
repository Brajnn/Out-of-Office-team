using MediatR;
using Out_of_Office.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Project.Command.CreateProject
{
    public class CreateProjectCommand:IRequest
    {
        public string Name { get; set; }
        public string ProjectType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ProjectManagerID { get; set; }
        public string? Comment { get; set; }
        public ProjectStatus Status { get; set; }
    }
}
