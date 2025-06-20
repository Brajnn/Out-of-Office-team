﻿using MediatR;
using Out_of_Office.Domain.Entities;
using Out_of_Office.Domain.Interfaces;
using System.Text.Json;
namespace Out_of_Office.Application.Employee.Command.CreateEmployee
{
    public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserService _userService;
        private readonly IAuditLogService _auditLogService;
        public CreateEmployeeCommandHandler(IEmployeeRepository employeeRepository, IUserService userService, IAuditLogService auditLogService)
        {
            _employeeRepository = employeeRepository;
            _userService = userService;
            _auditLogService = auditLogService;
        }

        public async Task<Unit> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        { 
            if (!Enum.TryParse(request.Status, out EmployeeStatus employeeStatus))
            {
                request.ValidationErrors = new List<string> { "Incorrect employee status." };
                return Unit.Value;
            }

            if (string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Subdivision) ||
                string.IsNullOrWhiteSpace(request.Position))
            {
                request.ValidationErrors = new List<string> { "Fill all fields." };
                return Unit.Value;
            }
            var (success, errors) = await _userService.CreateUserForEmployeeAsync(
                request.Username,
                request.Password,
                request.Position);
            if (!success)
            {
                request.ValidationErrors = errors.ToList();
                return Unit.Value;
            }
            if (request.VacationDays < 0 || request.SickLeaveDays < 0 || request.UnpaidLeaveDays < 0)
            {
                request.ValidationErrors = new List<string> { "Leave days cannot be negative." };
                return Unit.Value;
            }
            var employee = new Domain.Entities.Employee
            {
                FullName = request.FullName,
                Subdivision = request.Subdivision,
                Position = request.Position,
                HireDate = request.HireDate,
                Status = employeeStatus,
                PeoplePartnerID = request.PeoplePartnerID,
                OutOfOfficeBalance = request.OutOfOfficeBalance,
                Photo = request.Photo,
                LeaveBalances = new List<LeaveBalance>
                {
                    new LeaveBalance { Type = LeaveType.Vacation, DaysAvailable = request.VacationDays },
                    new LeaveBalance { Type = LeaveType.SickLeave, DaysAvailable = request.SickLeaveDays },
                    new LeaveBalance { Type = LeaveType.Unpaid, DaysAvailable = request.UnpaidLeaveDays }
                }
            };
            
            await _employeeRepository.AddEmployeeAsync(employee);
            await _userService.LinkUserToEmployeeAsync(request.Username, employee.Id);
            var details = JsonSerializer.Serialize(new
            {
                employee.Id,
                employee.FullName,
                employee.Position,
                employee.Status,
                employee.HireDate,
                request.Username
            });
            await _auditLogService.LogAsync("CreateEmployee", details, cancellationToken);
            return Unit.Value;
        }
    }
}
