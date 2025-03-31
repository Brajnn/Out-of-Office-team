using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Leave_Request.Command.UpdateLeaveRequestStatus
{
    public class UpdateLeaveRequestStatusCommand : IRequest
    {
        public int LeaveRequestID { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
    }
}
