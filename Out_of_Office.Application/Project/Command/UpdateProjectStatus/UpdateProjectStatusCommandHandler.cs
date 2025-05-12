using MediatR;
using Out_of_Office.Application.Common.Exceptions;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Project.Command.UpdateProjectStatus
{
    public class UpdateProjectStatusCommandHandler: IRequestHandler<UpdateProjectStatusCommand>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IAuditLogService _auditLogService;
        public UpdateProjectStatusCommandHandler(IProjectRepository projectRepository, IAuditLogService auditLogService)
        {
            _projectRepository = projectRepository;
            _auditLogService = auditLogService;
        }

        public async Task<Unit> Handle(UpdateProjectStatusCommand request, CancellationToken cancellationToken)
        {
            var project = await _projectRepository.GetProjectByIdAsync(request.ProjectId);
            if (project == null)
            {
                throw new NotFoundException(nameof(Project), request.ProjectId);
            }

            project.Status = request.Status;

            await _projectRepository.UpdateProjectAsync(project);
            var details = JsonSerializer.Serialize(new
            {
                ProjectId = project.ID,
                NewStatus = request.Status
            });

            await _auditLogService.LogAsync("UpdateProjectStatus", details, cancellationToken);

            return Unit.Value;
        }
    }
}
