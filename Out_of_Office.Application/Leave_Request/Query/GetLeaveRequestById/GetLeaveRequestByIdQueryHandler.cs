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
        private readonly IMapper _mapper;

        public GetLeaveRequestByIdQueryHandler(ILeaveRequestRepository leaveRequestRepository, IWorkCalendarRepository calendarRepository, IMapper mapper)
        {
            _leaveRequestRepository = leaveRequestRepository;
            _mapper = mapper;
            _calendarRepository = calendarRepository;
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
            return leaveRequestDto;
        }
    }
}
