using MediatR;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Project.Command.CreateProject
{
    public class CreateProjectCommandHandler:IRequestHandler<CreateProjectCommand>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IAuditLogService _auditLogService;
        public CreateProjectCommandHandler(IProjectRepository projectRepository, IAuditLogService auditLogService)
        {
            _projectRepository = projectRepository;
            _auditLogService = auditLogService;
        }

        public async Task<Unit> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            var project = new Domain.Entities.Project
            {
                ProjectType = request.ProjectType,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ProjectManagerID = request.ProjectManagerID,
                Comment = request.Comment,
                Status = request.Status
            };

            await _projectRepository.AddProjectAsync(project);
            
            var details = JsonSerializer.Serialize(new
            {
                project.ProjectType,
                project.StartDate,
                project.EndDate,
                project.ProjectManagerID,
                project.Status
            });

            await _auditLogService.LogAsync("CreateProject", details, cancellationToken);
            return Unit.Value;
        }

    }
}
