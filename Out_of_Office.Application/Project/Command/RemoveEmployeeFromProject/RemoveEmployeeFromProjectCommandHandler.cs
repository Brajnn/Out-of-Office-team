using MediatR;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Project.Command.RemoveEmployeeFromProject
{
    public class RemoveEmployeeFromProjectCommandHandler : IRequestHandler<RemoveEmployeeFromProjectCommand>
    {
        private readonly IEmployeeProjectRepository _employeeProjectRepository;
        private readonly IAuditLogService _auditLogService;
        public RemoveEmployeeFromProjectCommandHandler(IEmployeeProjectRepository employeeProjectRepository, IAuditLogService auditLogService)
        {
            _employeeProjectRepository = employeeProjectRepository;
            _auditLogService = auditLogService;
        }

        public async Task<Unit> Handle(RemoveEmployeeFromProjectCommand request, CancellationToken cancellationToken)
        {
            await _employeeProjectRepository.RemoveEmployeeProjectAsync(request.EmployeeId, request.ProjectId);
            var details = JsonSerializer.Serialize(new
            {
                request.EmployeeId,
                request.ProjectId
            });
            await _auditLogService.LogAsync("RemoveEmployeeFromProject", details, cancellationToken);

            return Unit.Value;
        }
    }
}
