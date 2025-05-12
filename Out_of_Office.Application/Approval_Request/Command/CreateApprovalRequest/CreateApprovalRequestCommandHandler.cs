using MediatR;
using Out_of_Office.Domain.Entities;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Approval_Request.Command.CreateApprovalRequest
{
    public class CreateApprovalRequestCommandHandler : IRequestHandler<CreateApprovalRequestCommand>
    {
        private readonly IApprovalRequestRepository _approvalRequestRepository;
        private readonly IAuditLogService _auditLogService;
        public CreateApprovalRequestCommandHandler(IApprovalRequestRepository approvalRequestRepository, IAuditLogService auditLogService)
        {
            _approvalRequestRepository = approvalRequestRepository;
            _auditLogService = auditLogService;
        }

        public async Task<Unit> Handle(CreateApprovalRequestCommand request, CancellationToken cancellationToken)
        {
            if (request.Status == default)
            {
                request.Status = ApprovalStatus.New;
            }

            var approvalRequest = new ApprovalRequest
            {
                ApproverID = request.ApproverID,
                LeaveRequestID = request.LeaveRequestID,
                Status = request.Status,
                Comment = request.Comment,

               
            };

            await _approvalRequestRepository.AddApprovalRequestAsync(approvalRequest);
            var details = JsonSerializer.Serialize(approvalRequest);
            await _auditLogService.LogAsync("CreateApprovalRequest", details, cancellationToken);
            return Unit.Value;
        }
    }
}
