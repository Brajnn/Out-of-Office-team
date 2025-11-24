using MediatR;
using Out_of_Office.Application.Leave_Request.Errors;
using Out_of_Office.Domain.Entities;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Leave_Request.Command.CreateLeaveRequestCommand
{
    public class CreateLeaveRequestCommandHandler : IRequestHandler<CreateLeaveRequestCommand>
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IWorkCalendarRepository _calendarRepository;
        private readonly IAuditLogService _auditLogService;
        public CreateLeaveRequestCommandHandler(
            ILeaveRequestRepository leaveRequestRepository,
            IEmployeeRepository employeeRepository,
            IWorkCalendarRepository calendarRepository,
            IAuditLogService auditLogService)
        {
            _leaveRequestRepository = leaveRequestRepository;
            _employeeRepository = employeeRepository;
            _calendarRepository = calendarRepository;
            _auditLogService = auditLogService;
        }

        public async Task<Unit> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            var today = DateTime.Today;

            // Validate the start date based on the selected absence reason
            switch (request.AbsenceReason)
            {
                case "Vacation":
                    if ((request.StartDate - today).TotalDays < 1)
                        throw new LeaveRequestValidationException(LeaveRequestError.VacationAdvanceError);
                    break;

                case "Unpaid":
                    if ((request.StartDate - today).TotalDays < 7)
                        throw new LeaveRequestValidationException(LeaveRequestError.UnpaidAdvanceError);
                    break;

                case "SickLeave":
                    if (request.StartDate < today)
                        throw new LeaveRequestValidationException(LeaveRequestError.SickLeavePastError);
                    break;

                default:
                    throw new LeaveRequestValidationException(LeaveRequestError.InvalidAbsenceReason);
            }
            if (request.EndDate < request.StartDate)
            {
                throw new LeaveRequestValidationException(LeaveRequestError.EndDateEarlierThanStart);
            }
            // Retrieve the employee based on the request's EmployeeId
            var employee = await _employeeRepository.GetEmployeeByIdAsync(request.EmployeeId);
            if (employee == null)
                throw new LeaveRequestValidationException(LeaveRequestError.EmployeeNotFound);

            // Retrieve the work calendar for the selected year
            var year = request.StartDate.Year;
            var calendar = await _calendarRepository.GetByYearAsync(year);
            if (calendar == null || !calendar.Any())
                throw new LeaveRequestValidationException(LeaveRequestError.NoCalendarForYear);

            // Calculate the number of working days between the start and end dates
            var workingDays = calendar
                .Where(d =>
                    d.Date >= request.StartDate &&
                    d.Date <= request.EndDate &&
                    !d.IsHoliday)
                .Count();

            // Check if the employee has enough available leave days
            var hasEnoughDays = request.AbsenceReason switch
            {
                "Vacation" => employee.LeaveBalances.Any(lb => lb.Type == LeaveType.Vacation && lb.DaysAvailable >= workingDays),
                "SickLeave" => employee.LeaveBalances.Any(lb => lb.Type == LeaveType.SickLeave && lb.DaysAvailable >= workingDays),
                "Unpaid" => employee.LeaveBalances.Any(lb => lb.Type == LeaveType.Unpaid && lb.DaysAvailable >= workingDays),
                _ => false
            };
            // Create and save the leave request
            if (!hasEnoughDays)
            {
                string displayName = request.AbsenceReason switch
                {
                    "Vacation" => "Vacation",
                    "SickLeave" => "Sick Leave",
                    "Unpaid" => "Unpaid Leave",
                    _ => request.AbsenceReason
                };

                throw new LeaveRequestValidationException(LeaveRequestError.InsufficientLeaveDays);
            }
            // Check for overlapping leave requests
            var allRequests = await _leaveRequestRepository.GetAllLeaveRequestsAsync();
            var overlappingRequestExists = allRequests.Any(lr =>
                lr.EmployeeID == request.EmployeeId &&
                lr.Status == LeaveRequest.AbsenceStatus.Approved &&
                lr.StartDate <= request.EndDate &&
                lr.EndDate >= request.StartDate
            );

            if (overlappingRequestExists)
            {
                throw new LeaveRequestValidationException(LeaveRequestError.OverlappingRequest);
            }
            var leaveRequest = new LeaveRequest
            {
                EmployeeID = request.EmployeeId,
                AbsenceReason = request.AbsenceReason,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Comment = request.Comment,
                Status = LeaveRequest.AbsenceStatus.New,
                CreatedAt = DateTime.Now
            };

            await _leaveRequestRepository.AddAsync(leaveRequest);
            var details = JsonSerializer.Serialize(new
            {
                leaveRequest.EmployeeID,
                leaveRequest.AbsenceReason,
                leaveRequest.StartDate,
                leaveRequest.EndDate,
                leaveRequest.Status,
                leaveRequest.CreatedAt
            });
            await _auditLogService.LogAsync("CreateLeaveRequest", details, cancellationToken);

            return Unit.Value;
        }
    }
}
