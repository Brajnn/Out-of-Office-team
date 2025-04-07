using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Domain.Entities
{
    public class LeaveRequest
    {
        public int ID { get; set; }
        public int EmployeeID { get; set; } 
        public Employee Employee { get; set; }
        public string AbsenceReason { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public AbsenceStatus Status { get; set; } = AbsenceStatus.New;
        public enum AbsenceStatus
        {
            New,
            Submitted,
            Approved,
            Cancelled,
            Rejected
        }
    }
}
