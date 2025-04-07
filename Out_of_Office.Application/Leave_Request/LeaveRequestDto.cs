using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Leave_Request
{
    public class LeaveRequestDto
    {
        public int ID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeFullName { get; set; } 
        public string AbsenceReason { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int WorkingDays { get; set; }
        public string Comment { get; set; }

        public DateTime? SubmittedAt { get; set; }
        public DateTime? StatusChangedAt { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
