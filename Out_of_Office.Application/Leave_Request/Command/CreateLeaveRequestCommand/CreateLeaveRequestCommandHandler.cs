using MediatR;
using Out_of_Office.Domain.Entities;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Leave_Request.Command.CreateLeaveRequestCommand
{
    public class CreateLeaveRequestCommandHandler : IRequestHandler<CreateLeaveRequestCommand>
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IWorkCalendarRepository _calendarRepository;

        public CreateLeaveRequestCommandHandler(
            ILeaveRequestRepository leaveRequestRepository,
            IEmployeeRepository employeeRepository,
            IWorkCalendarRepository calendarRepository)
        {
            _leaveRequestRepository = leaveRequestRepository;
            _employeeRepository = employeeRepository;
            _calendarRepository = calendarRepository;
        }

        public async Task<Unit> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            var today = DateTime.Today;

            // Step 1: Validate the start date based on the selected absence reason
            switch (request.AbsenceReason)
            {
                case "Vacation":
                    if ((request.StartDate - today).TotalDays < 1)
                        throw new ArgumentException("Vacation leave must be requested at least 1 day in advance.");
                    break;

                case "Unpaid":
                    if ((request.StartDate - today).TotalDays < 7)
                        throw new ArgumentException("Unpaid leave must be requested at least 7 days in advance.");
                    break;

                case "SickLeave":
                    if (request.StartDate < today)
                        throw new ArgumentException("Sick leave cannot start in the past.");
                    break;

                default:
                    throw new ArgumentException("Invalid absence reason selected.");
            }

            // Step 2: Retrieve the employee based on the request's EmployeeId
            var employee = await _employeeRepository.GetEmployeeByIdAsync(request.EmployeeId);
            if (employee == null)
                throw new Exception("Employee not found.");

            // Step 3: Retrieve the work calendar for the selected year
            var year = request.StartDate.Year;
            var calendar = await _calendarRepository.GetByYearAsync(year);
            if (calendar == null || !calendar.Any())
                throw new Exception($"No work calendar found for year {year}.");

            // Step 4: Calculate the number of working days between the start and end dates
            var workingDays = calendar
                .Where(d =>
                    d.Date >= request.StartDate &&
                    d.Date <= request.EndDate &&
                    !d.IsHoliday)
                .Count();

            // Step 5: Check if the employee has enough available leave days
            var hasEnoughDays = request.AbsenceReason switch
            {
                "Vacation" => employee.LeaveBalances.Any(lb => lb.Type == LeaveType.Vacation && lb.DaysAvailable >= workingDays),
                "SickLeave" => employee.LeaveBalances.Any(lb => lb.Type == LeaveType.SickLeave && lb.DaysAvailable >= workingDays),
                "Unpaid" => employee.LeaveBalances.Any(lb => lb.Type == LeaveType.Unpaid && lb.DaysAvailable >= workingDays),
                _ => false
            };
            // Step 6: Create and save the leave request
            if (!hasEnoughDays)
            {
                string displayName = request.AbsenceReason switch
                {
                    "Vacation" => "Vacation",
                    "SickLeave" => "Sick Leave",
                    "Unpaid" => "Unpaid Leave",
                    _ => request.AbsenceReason
                };

                throw new Exception($"Not enough available days for {displayName}. Please check your available days on your profile.");
            }

            var leaveRequest = new LeaveRequest
            {
                EmployeeID = request.EmployeeId,
                AbsenceReason = request.AbsenceReason,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Comment = request.Comment,
                Status = LeaveRequest.AbsenceStatus.New
            };

            await _leaveRequestRepository.AddAsync(leaveRequest);
            return Unit.Value;
        }
    }
}
