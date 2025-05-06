using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Approval_Request.Query.GetAllApprovalRequestQuery
{
    public class GetAllApprovalRequestQuery : IRequest<IEnumerable<ApprovalRequestDto>>
    {
        public int UserId { get; set; }
        public string UserRole { get; set; }
    }
}
