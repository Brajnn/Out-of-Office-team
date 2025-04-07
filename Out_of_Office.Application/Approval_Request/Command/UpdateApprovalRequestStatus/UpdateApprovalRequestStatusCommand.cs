using MediatR;
using Out_of_Office.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Approval_Request.Command.UpdateApprovalRequestStatus
{
    public class UpdateApprovalRequestStatusCommand : IRequest
    {
        public int ApprovalRequestId { get; set; }
        public DateTime? StatusChangedAt { get; set; }
        public ApprovalStatus Status { get; set; }
        public string DecisionComment { get; set; }
    }
}
