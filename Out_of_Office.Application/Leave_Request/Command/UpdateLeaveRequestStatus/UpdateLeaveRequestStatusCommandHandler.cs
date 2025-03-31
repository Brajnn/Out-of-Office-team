using MediatR;
using Out_of_Office.Domain.Entities;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Leave_Request.Command.UpdateLeaveRequestStatus
{
    public class UpdateLeaveRequestStatusCommandHandler : IRequestHandler<UpdateLeaveRequestStatusCommand>
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IApprovalRequestRepository _approvalRequestRepository;

        public UpdateLeaveRequestStatusCommandHandler(ILeaveRequestRepository leaveRequestRepository, IApprovalRequestRepository approvalRequestRepository)
        {
            _leaveRequestRepository = leaveRequestRepository;
            _approvalRequestRepository = approvalRequestRepository;
        }

        public async Task<Unit> Handle(UpdateLeaveRequestStatusCommand request, CancellationToken cancellationToken)
        {
            var leaveRequest = await _leaveRequestRepository.GetLeaveRequestByIdAsync(request.LeaveRequestID);
            if (leaveRequest == null)
            {
                throw new KeyNotFoundException("LeaveRequest not found.");
            }

            if (!Enum.TryParse(request.Status, out LeaveRequest.AbsenceStatus newStatus))
            {
                throw new ArgumentException("Invalid status.");
            }

            leaveRequest.Status = newStatus;
            await _leaveRequestRepository.UpdateLeaveRequestAsync(leaveRequest);

            if (newStatus == LeaveRequest.AbsenceStatus.Submitted)
            {
                var existingApprovalRequest = await _approvalRequestRepository.GetApprovalRequestByLeaveRequestIdAsync(request.LeaveRequestID);
                if (existingApprovalRequest == null)
                {
                    var approvalRequest = new ApprovalRequest
                    {
                        LeaveRequestID = request.LeaveRequestID,
                        ApproverID = leaveRequest.Employee.PeoplePartnerID,
                        Status = ApprovalStatus.New,
                        Comment = leaveRequest.Comment
                    };
                    await _approvalRequestRepository.AddApprovalRequestAsync(approvalRequest);
                }
            }

            return Unit.Value;
        }
    }
    
}
