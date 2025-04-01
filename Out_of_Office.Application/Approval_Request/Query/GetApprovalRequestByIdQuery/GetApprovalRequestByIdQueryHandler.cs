using AutoMapper;
using MediatR;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Approval_Request.Query.GetApprovalRequestByIdQuery
{
    public class GetApprovalRequestByIdQueryHandler : IRequestHandler<GetApprovalRequestByIdQuery, ApprovalRequestDto>
    {
        private readonly IApprovalRequestRepository _approvalRequestRepository;
        private readonly IMapper _mapper;

        public GetApprovalRequestByIdQueryHandler(IApprovalRequestRepository approvalRequestRepository, IMapper mapper)
        {
            _approvalRequestRepository = approvalRequestRepository;
            _mapper = mapper;
        }

        public async Task<ApprovalRequestDto> Handle(GetApprovalRequestByIdQuery request, CancellationToken cancellationToken)
        {
            var approvalRequest = await _approvalRequestRepository.GetApprovalRequestByIdAsync(request.Id);
            var dto = _mapper.Map<ApprovalRequestDto>(approvalRequest);
            dto.LeaveRequestSubmittedAt= approvalRequest.LeaveRequest.SubmittedAt;
            return dto;
        }
    }
}
