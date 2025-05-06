using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        public EmployeeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ======================= LISTA PRACOWNIKÓW ==========================
        public async Task<IActionResult> Index(string sortOrder, string searchString, string positionFilter, int? pageNumber)
        {
            var employees = await _mediator.Send(new GetAllEmployeesQuery());

            if (!string.IsNullOrEmpty(searchString))
                employees = employees.Where(e => e.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrEmpty(positionFilter))
                employees = employees.Where(e => e.Position == positionFilter).ToList();

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
            ViewBag.CurrentPositionFilter = positionFilter;
            ViewBag.Positions = new SelectList(new[] { "HR Manager", "Project Manager", "Employee" });

            return View(employees.ToPagedList(pageNumber ?? 1, 10));
        }

        // ======================= SZCZEGÓŁY ==========================
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

        // ======================= UPDATE STATUS ==========================
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

        // ======================= EDIT ==========================
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

        // ======================= PROFIL ==========================
        [HttpGet]
        public async Task<IActionResult> EmployeeProfile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int employeeId))
                return Unauthorized();

            var employee = await _mediator.Send(new GetEmployeeByIdQuery { Id = employeeId });
            return employee is null ? NotFound("Employee not found.") : View("EmployeeProfile", employee);
        }
    }
}
