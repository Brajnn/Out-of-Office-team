using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Domain.Entities
{
    public class ApprovalRequest
    {
        public int ID { get; set; }
        public int ApproverID { get; set; }  
        public Employee Approver { get; set; }
        public int LeaveRequestID { get; set; }  
        public LeaveRequest LeaveRequest { get; set; }
        public ApprovalStatus Status { get; set; } = ApprovalStatus.New;
        public DateTime? StatusChangedAt { get; set; }
        public string? Comment { get; set; } 
        public string? DecisionComment { get; set; }
    }

    public enum ApprovalStatus
    {
        New,
        Approved,
        Rejected
    }

}
