using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Out_of_Office.Application.Employee;
using Out_of_Office.Application.Employee.Command.CreateEmployee;
using Out_of_Office.Application.Employee.Command.UpdateEmployeeCommand;
using Out_of_Office.Application.Employee.Command.UpdateEmployeeStatus;
using Out_of_Office.Application.Employee.Queries.GetAllEmployees;
using Out_of_Office.Application.Employee.Queries.GetEmployeeById;
using Out_of_Office.Application.Leave_Balance;
using Out_of_Office.Application.Models.AssignProjectViewModel;
using Out_of_Office.Application.Project.Query.GetAllProjectsQuery;
using Out_of_Office.Application.User.Query.GetUserById;
using Out_of_Office.Domain.Interfaces;
using Out_of_Office.Infrastructure.Presistance;
using System.Security.Claims;
using X.PagedList;
namespace Out_of_Office.Controllers
{
    [Authorize(Roles = "Employee,HRManager,ProjectManager,Administrator")]
    public class EmployeeController :Controller
    {
        private readonly IMediator _mediator;

        public EmployeeController(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IActionResult> Index(string sortOrder, string searchString, string positionFilter, int? pageNumber)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.IdSortParm = String.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewBag.NameSortParm = sortOrder == "Name" ? "name_desc" : "Name";
            ViewBag.PositionSortParm = sortOrder == "Position" ? "position_desc" : "Position";


            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentPositionFilter = positionFilter;

            var employees = await _mediator.Send(new GetAllEmployeesQuery());

            if (!String.IsNullOrEmpty(searchString))
            {
                employees = employees.Where(e => e.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!String.IsNullOrEmpty(positionFilter))
            {
                employees = employees.Where(e => e.Position == positionFilter).ToList();
            }
            employees = sortOrder switch
            {             
                "Name" => employees.OrderBy(e => e.FullName).ToList(),
                "name_desc" => employees.OrderByDescending(e => e.FullName).ToList(),             
                "Position" => employees.OrderBy(e => e.Position).ToList(),
                "position_desc" => employees.OrderByDescending(e => e.Position).ToList(),   
                _=>employees
            };
            int pageSize = 10;
            int pageIndex = pageNumber ?? 1;
            ViewBag.Positions = new SelectList(new[] { "HR Manager", "Project Manager", "Employee" });
            return View(employees.ToPagedList(pageIndex, pageSize));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var employee = await _mediator.Send(new GetEmployeeByIdQuery { Id = id });
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var employees = await _mediator.Send(new GetAllEmployeesQuery());
            var hrManagers = employees.Where(e => e.Position == "HR Manager").ToList();
            var hireDate = DateTime.Now;
            ViewBag.HrManagers = hrManagers;
            ViewBag.Positions = new[] { "HR Manager", "Project Manager", "Employee" };

            return View("CreateEmployee", new CreateEmployeeCommand() { HireDate=hireDate} );
        }

        
        [HttpPost]
        public async Task<IActionResult> Create(CreateEmployeeCommand command)
        {
            if (ModelState.IsValid)
            {
                await _mediator.Send(command);
                return RedirectToAction(nameof(Index));
            }
            var employees = await _mediator.Send(new GetAllEmployeesQuery());
            var hrManagers = employees.Where(e => e.Position == "HR Manager").ToList();

            ViewBag.HrManagers = hrManagers;
            ViewBag.Positions = new[] { "HR Manager", "Project Manager", "Employee" };


            return View("CreateEmployee", command);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, bool isActive)
        {
            var command = new UpdateEmployeeStatusCommand
            {
                Id = id,
                Status = isActive ? "Active" : "Inactive"
            };
            await _mediator.Send(command);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var query = new GetEmployeeByIdQuery { Id = id };
            var employee = await _mediator.Send(query);
            if (employee == null)
            {
                return NotFound();
            }
            return View("EditEmployee", employee);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EmployeeDto employeeDto)
        {
            if (ModelState.IsValid)
            {
                var command = new UpdateEmployeeCommand
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
                    LeaveBalances = employeeDto.LeaveBalances?
                    .Select(lb => new LeaveBalanceDto
                    {
                        Type = lb.Type,
                        DaysAvailable = lb.DaysAvailable
                    })
                    .ToList() ?? new List<LeaveBalanceDto>()
                    };

                await _mediator.Send(command);
                return RedirectToAction(nameof(Index));
            }
            return View("EditEmployee", employeeDto);
        }

        [HttpGet]
        public async Task<IActionResult> EmployeeProfile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var user = await _mediator.Send(new GetUserByIdQuery { Id = userId });
            if (user == null)
                return NotFound("User not found.");

            var employee = await _mediator.Send(new GetEmployeeByIdQuery { Id = user.EmployeeId });
            if (employee == null)
                return NotFound("Employee not found.");

            return View("EmployeeProfile", employee);
        }

    }
}
