using MediatR;
using Out_of_Office.Domain.Entities;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Project.Command.AssignEmployee
{
    public class AssignEmployeeCommandHandler : IRequestHandler<AssignEmployeeCommand>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IEmployeeProjectRepository _employeeProjectRepository;
        private readonly IAuditLogService _auditLogService;
        public AssignEmployeeCommandHandler(IProjectRepository projectRepository, IEmployeeProjectRepository employeeProjectRepository, IAuditLogService auditLogService)
        {
            _projectRepository = projectRepository;
            _employeeProjectRepository = employeeProjectRepository;
            _auditLogService = auditLogService;
        }

        public async Task<Unit> Handle(AssignEmployeeCommand request, CancellationToken cancellationToken)
        {
            var project = await _projectRepository.GetProjectByIdAsync(request.ProjectId);
            if (project == null)
            {
                throw new KeyNotFoundException("Project not found.");
            }

            var employeeProject = await _employeeProjectRepository.GetEmployeeProjectAsync(request.EmployeeId, request.ProjectId);
            if (employeeProject != null)
            {
                // Employee is already assigned to the project, so we don't need to add again.
                return Unit.Value;
            }

            var newEmployeeProject = new EmployeeProject
            {
                EmployeeId = request.EmployeeId,
                ProjectId = request.ProjectId
            };

            await _employeeProjectRepository.AddEmployeeProjectAsync(newEmployeeProject);
            var details = JsonSerializer.Serialize(new
            {
                EmployeeId = request.EmployeeId,
                ProjectId = request.ProjectId
            });

            await _auditLogService.LogAsync("AssignEmployeeToProject", details, cancellationToken);

            return Unit.Value;
        }
    }
}
