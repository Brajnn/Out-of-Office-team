using MediatR;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Employee.Command.UpdateEmployeeStatus
{
    public class UpdateEmployeeStatusCommandHandler:IRequestHandler<UpdateEmployeeStatusCommand>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IAuditLogService _auditLogService;
        public UpdateEmployeeStatusCommandHandler(IEmployeeRepository employeeRepository, IAuditLogService auditLogService)
        {
            _employeeRepository = employeeRepository;
            _auditLogService = auditLogService;
        }

        public async Task<Unit> Handle(UpdateEmployeeStatusCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(request.Id);
            if (employee == null)
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            if (!Enum.TryParse(request.Status, out Domain.Entities.EmployeeStatus employeeStatus))
            {
                throw new ArgumentException("Invalid status.");
            }

            employee.Status = employeeStatus;
            await _employeeRepository.UpdateEmployeeAsync(employee);
            var details = JsonSerializer.Serialize(new
            {
                employee.Id,
                NewStatus = employeeStatus.ToString()
            });
            await _auditLogService.LogAsync("UpdateEmployeeStatus", details, cancellationToken);

            return Unit.Value;
        }
    }
}
