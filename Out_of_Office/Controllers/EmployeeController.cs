using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Out_of_Office.Application.Employee;
using Out_of_Office.Application.Employee.Command.CreateEmployee;
using Out_of_Office.Application.Employee.Command.UpdateEmployeeCommand;
using Out_of_Office.Application.Employee.Command.UpdateEmployeeStatus;
using Out_of_Office.Application.Employee.Queries.GetAllEmployees;
using Out_of_Office.Application.Employee.Queries.GetEmployeeById;
using Out_of_Office.Application.Leave_Balance;
using Out_of_Office.Infrastructure.Identity;
using System.Security.Claims;
using X.PagedList;
namespace Out_of_Office.Controllers
{
    [Authorize(Roles = "Employee,HRManager,ProjectManager,Administrator")]
    public class EmployeeController : Controller
    {
        private readonly IMediator _mediator;
        private readonly UserManager<ApplicationUser> _userManager;
        public EmployeeController(IMediator mediator, UserManager<ApplicationUser> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(string sortOrder, string searchString, List<string> selectedPositions, bool showInactive = false, int? pageNumber = 1)
        {
            var employees = await _mediator.Send(new GetAllEmployeesQuery());
            // Filter by name
            if (!string.IsNullOrEmpty(searchString))
                employees = employees.Where(e => e.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            // Filter by positions
            if (selectedPositions != null && selectedPositions.Any())
                employees = employees.Where(e => selectedPositions.Contains(e.Position)).ToList();

            // Filter by status
            if (!showInactive)
                employees = employees.Where(e => e.Status == "Active").ToList();

            employees = sortOrder switch
            {
                "Name" => employees.OrderBy(e => e.FullName).ToList(),
                "name_desc" => employees.OrderByDescending(e => e.FullName).ToList(),
                "Position" => employees.OrderBy(e => e.Position).ToList(),
                "position_desc" => employees.OrderByDescending(e => e.Position).ToList(),
                _ => employees
            };

            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentFilter = searchString;
            ViewBag.ShowInactive = showInactive;
            ViewBag.SelectedPositions = selectedPositions ?? new List<string>();
            ViewBag.Positions = new List<SelectListItem>
            {
                new SelectListItem { Text = "HR Manager", Value = "HRManager" },
                new SelectListItem { Text = "Project Manager", Value = "ProjectManager" },
                new SelectListItem { Text = "Employee", Value = "Employee" }
            };
            return View(employees.ToPagedList(pageNumber ?? 1, 10));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var employee = await _mediator.Send(new GetEmployeeByIdQuery { Id = id });
            return employee is null ? NotFound() : View(employee);
        }


        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create()
        {
            await PrepareCreateEmployeeViewDataAsync();
            return View("CreateEmployee", new CreateEmployeeCommand { HireDate = DateTime.Now });
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(CreateEmployeeCommand command)
        {
            if (!ModelState.IsValid)
            {
                await PrepareCreateEmployeeViewDataAsync();
                return View("CreateEmployee", command);
            }

            await _mediator.Send(command);

            if (command.ValidationErrors.Any())
            {
                foreach (var error in command.ValidationErrors)
                    ModelState.AddModelError(string.Empty, error);

                await PrepareCreateEmployeeViewDataAsync();
                return View("CreateEmployee", command);
            }

            return RedirectToAction(nameof(Index));
        }
        private async Task PrepareCreateEmployeeViewDataAsync()
        {
            var employees = await _mediator.Send(new GetAllEmployeesQuery());

            var hrManagers = employees
                .Where(e => e.Position == "HRManager")
                .Select(e => new SelectListItem
                {
                    Text = e.FullName,
                    Value = e.Id.ToString()
                }).ToList();

            var positions = new List<SelectListItem>
            {
                new SelectListItem { Text = "HR Manager", Value = "HRManager" },
                new SelectListItem { Text = "Project Manager", Value = "ProjectManager" },
                new SelectListItem { Text = "Employee", Value = "Employee" }
            };

            ViewBag.HrManagers = hrManagers;
            ViewBag.Positions = positions;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, bool isActive)
        {
            await _mediator.Send(new UpdateEmployeeStatusCommand
            {
                Id = id,
                Status = isActive ? "Active" : "Inactive"
            });

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _mediator.Send(new GetEmployeeByIdQuery { Id = id });
            return employee is null ? NotFound() : View("EditEmployee", employee);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EmployeeDto employeeDto)
        {
            if (!ModelState.IsValid)
                return View("EditEmployee", employeeDto);

            await _mediator.Send(new UpdateEmployeeCommand
            {
                Id = employeeDto.Id,
                FullName = employeeDto.FullName,
                Subdivision = employeeDto.Subdivision,
                Position = employeeDto.Position,
                Status = employeeDto.Status,
                PeoplePartnerID = employeeDto.PeoplePartnerID,
                OutOfOfficeBalance = employeeDto.OutOfOfficeBalance,
                HireDate = employeeDto.HireDate,
                Photo = employeeDto.Photo,
                LeaveBalances = employeeDto.LeaveBalances?.Select(lb => new LeaveBalanceDto
                {
                    Type = lb.Type,
                    DaysAvailable = lb.DaysAvailable
                }).ToList() ?? new List<LeaveBalanceDto>()
            });

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> EmployeeProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.EmployeeId == 0)
                return Unauthorized();

            var employee = await _mediator.Send(new GetEmployeeByIdQuery { Id = user.EmployeeId.Value });
            if (employee == null)
                return NotFound("Employee not found.");

            ViewBag.Username = user.UserName;
            ViewBag.HasAuthenticator = user.TwoFactorEnabled;
            return View("EmployeeProfile", employee);
        }
    }
}
