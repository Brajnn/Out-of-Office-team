using MediatR;
using Out_of_Office.Domain.Entities;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Leave_Request.Command.UpdateLeaveRequestStatus
{
    public class UpdateLeaveRequestStatusCommandHandler : IRequestHandler<UpdateLeaveRequestStatusCommand>
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IApprovalRequestRepository _approvalRequestRepository;
        private readonly IAuditLogService _auditLogService;
        public UpdateLeaveRequestStatusCommandHandler(ILeaveRequestRepository leaveRequestRepository, IApprovalRequestRepository approvalRequestRepository, IAuditLogService auditLogService )
        {
            _leaveRequestRepository = leaveRequestRepository;
            _approvalRequestRepository = approvalRequestRepository;
            _auditLogService = auditLogService;
        }

        public async Task<Unit> Handle(UpdateLeaveRequestStatusCommand request, CancellationToken cancellationToken)
        {
            // Validate leave request
            var leaveRequest = await _leaveRequestRepository.GetLeaveRequestByIdAsync(request.LeaveRequestID);
            if (leaveRequest == null)
            {
                throw new KeyNotFoundException("LeaveRequest not found.");
            }
            // Parse and set status
            if (!Enum.TryParse(request.Status, out LeaveRequest.AbsenceStatus newStatus))
            {
                throw new ArgumentException("Invalid status.");
            }

            leaveRequest.Status = newStatus;

            // Handle Submitted status
            if (newStatus == LeaveRequest.AbsenceStatus.Submitted)
            {
                // Set submission timestamp if not already set
                if (leaveRequest.SubmittedAt == null)
                    leaveRequest.SubmittedAt = DateTime.Now;
                // Only create approval request if it doesn't exist
                var existingApprovalRequest = await _approvalRequestRepository.GetApprovalRequestByLeaveRequestIdAsync(request.LeaveRequestID);

                if (existingApprovalRequest == null)
                {
                    var approvalRequest = new ApprovalRequest
                    {
                        LeaveRequestID = request.LeaveRequestID,
                        ApproverID = leaveRequest.Employee.PeoplePartnerID,
                        Status = ApprovalStatus.New,
                        Comment = leaveRequest.Comment,
                         
                    };
                    
                    await _approvalRequestRepository.AddApprovalRequestAsync(approvalRequest);
                    var approvalDetails = JsonSerializer.Serialize(new
                    {
                        approvalRequest.LeaveRequestID,
                        approvalRequest.ApproverID,
                        approvalRequest.Status
                    });
                    await _auditLogService.LogAsync("CreateApprovalRequestForSubmittedLeave", approvalDetails, cancellationToken);
                }
            }
            await _leaveRequestRepository.UpdateLeaveRequestAsync(leaveRequest);

            return Unit.Value;
        }
    }
    
}
