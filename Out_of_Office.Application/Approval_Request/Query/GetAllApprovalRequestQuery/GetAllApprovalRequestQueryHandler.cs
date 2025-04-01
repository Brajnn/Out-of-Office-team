using AutoMapper;
using MediatR;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Approval_Request.Query.GetAllApprovalRequestQuery
{
    public class GetAllApprovalRequestQueryHandler : IRequestHandler<GetAllApprovalRequestQuery, IEnumerable<ApprovalRequestDto>>
    {
        private readonly IApprovalRequestRepository _approvalRequestRepository;
        private readonly IMapper _mapper;

        public GetAllApprovalRequestQueryHandler(IApprovalRequestRepository approvalRequestRepository, IMapper mapper)
        {
            _approvalRequestRepository = approvalRequestRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ApprovalRequestDto>> Handle(GetAllApprovalRequestQuery request, CancellationToken cancellationToken)
        {
            var approvalRequests = await _approvalRequestRepository.GetAllApprovalRequestsAsync();

            return _mapper.Map<IEnumerable<ApprovalRequestDto>>(approvalRequests);
        }
    }
}
