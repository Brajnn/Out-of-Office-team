using MediatR;
using Out_of_Office.Domain.Entities;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Approval_Request.Command.CreateApprovalRequest
{
    public class CreateApprovalRequestCommandHandler : IRequestHandler<CreateApprovalRequestCommand>
    {
        private readonly IApprovalRequestRepository _approvalRequestRepository;

        public CreateApprovalRequestCommandHandler(IApprovalRequestRepository approvalRequestRepository)
        {
            _approvalRequestRepository = approvalRequestRepository;
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

            return Unit.Value;
        }
    }
}
