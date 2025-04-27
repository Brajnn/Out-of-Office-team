using AutoMapper;
using MediatR;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Leave_Request.Query.GetAllLeaveRequest
{
    public class GetAllLeaveRequestsQueryHandler : IRequestHandler<GetAllLeaveRequestsQuery, IEnumerable<LeaveRequestDto>>
    {
        private readonly IApprovalRequestRepository _approvalRequestRepository;
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IMapper _mapper;

        public GetAllLeaveRequestsQueryHandler(ILeaveRequestRepository leaveRequestRepository, IMapper mapper, IApprovalRequestRepository approvalRequestRepository)
        {
            _approvalRequestRepository = _approvalRequestRepository = approvalRequestRepository; ;
            _leaveRequestRepository = leaveRequestRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LeaveRequestDto>> Handle(GetAllLeaveRequestsQuery request, CancellationToken cancellationToken)
        {
            var leaveRequests = await _leaveRequestRepository.GetAllLeaveRequestsAsync();

            foreach (var leaveRequest in leaveRequests)
            {
                if (leaveRequest.Status == Domain.Entities.LeaveRequest.AbsenceStatus.Submitted
                    && leaveRequest.StartDate < DateTime.Today)
                {
                    leaveRequest.Status = Domain.Entities.LeaveRequest.AbsenceStatus.Cancelled;
                    await _leaveRequestRepository.UpdateLeaveRequestAsync(leaveRequest);
                }
            }
            if (request.UserRole == "Employee")
            {
                leaveRequests = leaveRequests.Where(lr => lr.Employee.Id == request.UserId).ToList();
            }
            var leaveRequestDtos = _mapper.Map<IEnumerable<LeaveRequestDto>>(leaveRequests);
            return leaveRequestDtos;
        }
    }
}
