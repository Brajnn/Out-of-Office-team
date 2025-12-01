using AutoMapper;
using MediatR;
using Out_of_Office.Application.Employee;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Project.Query.GetProjectById
{
    public class GetProjectByIdQueryHandler:IRequestHandler<GetProjectByIdQuery,ProjectDto>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;

        public GetProjectByIdQueryHandler(IProjectRepository projectRepository, IMapper mapper)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<ProjectDto> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
        {
            var project = await _projectRepository.GetProjectByIdAsync(request.Id);
            if (project == null)
            {
                throw new KeyNotFoundException("Project not found.");
            }

            var projectDto = _mapper.Map<ProjectDto>(project);


            projectDto.Employees = project.EmployeeProjects.Select(ep => new EmployeeDto
            {
                Id = ep.Employee.Id, 
                FullName = ep.Employee.FullName,
                Subdivision = ep.Employee.Subdivision,
                Position = ep.Employee.Position,
                Status = ep.Employee.Status.ToString(),
                PeoplePartnerID = ep.Employee.PeoplePartnerID,
                OutOfOfficeBalance = ep.Employee.OutOfOfficeBalance
            }).ToList();

            return projectDto;
        }
    }
}
