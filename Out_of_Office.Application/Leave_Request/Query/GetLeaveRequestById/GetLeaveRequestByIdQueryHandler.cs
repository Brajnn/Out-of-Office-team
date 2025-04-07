using AutoMapper;
using MediatR;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Leave_Request.Query.GetLeaveRequestById
{
    public class GetLeaveRequestByIdQueryHandler:IRequestHandler<GetLeaveRequestByIdQuery,LeaveRequestDto>
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IWorkCalendarRepository _calendarRepository;
        private readonly IApprovalRequestRepository _approvalRequestRepository;
        private readonly IMapper _mapper;

        public GetLeaveRequestByIdQueryHandler(ILeaveRequestRepository leaveRequestRepository, IWorkCalendarRepository calendarRepository,  IMapper mapper, IApprovalRequestRepository approvalRequestRepository)
        {
            _leaveRequestRepository = leaveRequestRepository;
            _mapper = mapper;
            _calendarRepository = calendarRepository;
            _approvalRequestRepository = approvalRequestRepository;
        }

        public async Task<LeaveRequestDto> Handle(GetLeaveRequestByIdQuery request, CancellationToken cancellationToken)
        {
            
            var leaveRequest = await _leaveRequestRepository.GetLeaveRequestByIdAsync(request.Id);
            if (leaveRequest == null)
            {
                throw new KeyNotFoundException($"LeaveRequest with ID {request.Id} not found.");
            }
            var calendarDays = await _calendarRepository.GetByYearAsync(leaveRequest.StartDate.Year);
            int workingDays = calendarDays
                .Where(d => d.Date >= leaveRequest.StartDate && d.Date <= leaveRequest.EndDate && !d.IsHoliday)
                .Count();


            var leaveRequestDto = _mapper.Map<LeaveRequestDto>(leaveRequest);
            leaveRequestDto.WorkingDays = workingDays;
            var approvalRequest = await _approvalRequestRepository.GetApprovalRequestByLeaveRequestIdAsync(leaveRequest.ID);
            if (approvalRequest != null)
            {
                leaveRequestDto.DecisionComment = approvalRequest.DecisionComment;
            }
            leaveRequestDto.StatusChangedAt = approvalRequest?.StatusChangedAt;

            return leaveRequestDto;
        }
    }
}
