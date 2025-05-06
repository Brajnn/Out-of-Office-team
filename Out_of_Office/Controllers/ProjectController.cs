using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Out_of_Office.Application.Employee.Queries.GetAllEmployees;
using Out_of_Office.Application.Models.AssignProjectViewModel;
using Out_of_Office.Application.Project.Command.AssignEmployee;
using Out_of_Office.Application.Project.Command.CreateProject;
using Out_of_Office.Application.Project.Command.RemoveEmployeeFromProject;
using Out_of_Office.Application.Project.Command.UpdateProjectStatus;
using Out_of_Office.Application.Project.Query.GetAllProjectsQuery;
using Out_of_Office.Application.Project.Query.GetProjectById;
using Out_of_Office.Domain.Entities;
using Out_of_Office.Domain.Interfaces;
using System.Security.Claims;
using X.PagedList;

namespace Out_of_Office.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(IMediator mediator, IEmployeeRepository employeeRepository, ILogger<ProjectController> logger)
        {
            _mediator = mediator;
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? searchProjectId, string sortOrder, int? pageNumber)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.IdSortParm = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewBag.TypeSortParm = sortOrder == "type_asc" ? "type_desc" : "type_asc";
            ViewBag.StartDateSortParm = sortOrder == "startdate_asc" ? "startdate_desc" : "startdate_asc";
            ViewBag.ManagerSortParm = sortOrder == "manager_asc" ? "manager_desc" : "manager_asc";

            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var employee = await _employeeRepository.GetEmployeeByApplicationUserIdAsync(appUserId);
            if (employee == null)
            {
                _logger.LogWarning("No employee found for ApplicationUser ID: {AppUserId}", appUserId);
                return Unauthorized();
            }
            var employeeId = employee.Id;

            var projects = await _mediator.Send(new GetAllProjectsQuery { UserId = employeeId, UserRole = userRole });
            if (searchProjectId.HasValue)
            {
                projects = projects.Where(p => p.ID == searchProjectId.Value).ToList();
                ViewData["SearchProjectId"] = searchProjectId.Value;
            }
            projects = sortOrder switch
            {
                "id_desc" => projects.OrderByDescending(p => p.ID).ToList(),
                "type_asc" => projects.OrderBy(p => p.ProjectType).ToList(),
                "type_desc" => projects.OrderByDescending(p => p.ProjectType).ToList(),
                "startdate_asc" => projects.OrderBy(p => p.StartDate).ToList(),
                "startdate_desc" => projects.OrderByDescending(p => p.StartDate).ToList(),
                "manager_asc" => projects.OrderBy(p => p.ProjectManagerFullName).ToList(),
                "manager_desc" => projects.OrderByDescending(p => p.ProjectManagerFullName).ToList(),
                _ => projects.OrderBy(p => p.ID).ToList(),
            };

            int pageSize = 10;
            int pageIndex = pageNumber ?? 1;

            var pagedList = projects.ToPagedList(pageIndex, pageSize);

            return View(pagedList);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var project = await _mediator.Send(new GetProjectByIdQuery { Id = id });
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var projectManagers = await _employeeRepository.GetProjectManagersAsync();
            ViewBag.ProjectManagers = projectManagers;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProjectCommand command)
        {
            if (ModelState.IsValid)
            {
                await _mediator.Send(command);
                return RedirectToAction(nameof(Index));
            }
            var projectManagers = await _employeeRepository.GetProjectManagersAsync();
            ViewBag.ProjectManagers = projectManagers;
            return View(command);
        }
        [HttpPost]
        public async Task<IActionResult> Activate(int id)
        {
            var updateProjectStatusCommand = new UpdateProjectStatusCommand
            {
                ProjectId = id,
                Status = ProjectStatus.Active
            };

            await _mediator.Send(updateProjectStatusCommand);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(int id)
        {
            var updateProjectStatusCommand = new UpdateProjectStatusCommand
            {
                ProjectId = id,
                Status = ProjectStatus.Inactive
            };

            await _mediator.Send(updateProjectStatusCommand);
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "ProjectManager,Administrator")]
        [HttpGet]
        public async Task<IActionResult> AssignEmployee(int id)
        {
            var project = await _mediator.Send(new GetProjectByIdQuery { Id = id });
            var employees = await _mediator.Send(new GetAllEmployeesQuery());
            var activeEmployees = employees.Where(e => e.Status == "Active").ToList();
            var viewModel = new AssignProjectViewModel
            {
                ProjectId = id,
                Project = project,
                Employees = activeEmployees
            };

            return View(viewModel);
        }

        [Authorize(Roles = "ProjectManager,Administrator")]
        [HttpPost]
        public async Task<IActionResult> AssignEmployee(AssignProjectViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var command = new AssignEmployeeCommand
                {
                    ProjectId = viewModel.ProjectId,
                    EmployeeId = viewModel.SelectedEmployeeId,

                };

                System.Diagnostics.Debug.WriteLine($"ProjectId: {command.ProjectId}, EmployeeId: {command.EmployeeId}");

                await _mediator.Send(command);
                return RedirectToAction(nameof(Details), new { id = command.ProjectId });
            }

            var project = await _mediator.Send(new GetProjectByIdQuery { Id = viewModel.ProjectId });
            var employees = await _mediator.Send(new GetAllEmployeesQuery());

            viewModel.Project = project;
            viewModel.Employees = employees.ToList();

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> RemoveEmployee(RemoveEmployeeFromProjectCommand command)
        {
            if (ModelState.IsValid)
            {
                await _mediator.Send(command);
                return RedirectToAction(nameof(Details), new { id = command.ProjectId });
            }
            return RedirectToAction(nameof(Details), new { id = command.ProjectId });
        }

    }
    
}
