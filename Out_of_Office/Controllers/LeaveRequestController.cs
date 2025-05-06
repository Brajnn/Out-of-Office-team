using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Out_of_Office.Application.Leave_Request.Query.GetAllLeaveRequest;
using Out_of_Office.Application.Leave_Request.Query.GetLeaveRequestById;
using Out_of_Office.Domain.Interfaces;
using System.Security.Claims;

using Out_of_Office.Application.Leave_Request.Command.CreateLeaveRequestCommand;
using Out_of_Office.Application.Leave_Request.Command.UpdateLeaveRequestStatus;
using X.PagedList;
using Out_of_Office.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Out_of_Office.Controllers
{
    [Authorize]
    public class LeaveRequestController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<LeaveRequestController> _logger;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public LeaveRequestController(IMediator mediator, ILogger<LeaveRequestController> logger, IEmployeeRepository employeeRepository, UserManager<ApplicationUser> userManager)
        {
            _mediator = mediator;
            _logger = logger;
            _employeeRepository = employeeRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? searchRequestId, string sortOrder, int? pageNumber, List<string> selectedStatuses, DateTime? startDate, DateTime? endDate)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.IdSortParm = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewBag.EmployeeSortParm = sortOrder == "employee_asc" ? "employee_desc" : "employee_asc";
            ViewBag.StartDateSortParm = sortOrder == "startdate_asc" ? "startdate_desc" : "startdate_asc";
            ViewBag.CreatedAtSortParm = sortOrder == "createdat_asc" ? "createdatdate_desc" : "createdatdate_asc";
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var applicationUser = await _userManager.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Id == userId);

            if (applicationUser == null)
            {
                _logger.LogWarning("User not found for ID: {UserId}", userId);
                return Unauthorized();
            }

            var employee = applicationUser.Employee;

            if (employee == null && userRole == "Employee")
            {
                _logger.LogWarning("No employee assigned to user ID: {UserId}", userId);
                return Unauthorized();
            }

            var leaveRequests = await _mediator.Send(new GetAllLeaveRequestsQuery
            {
                UserId = employee?.Id ?? 0,
                UserRole = userRole
            });

            // --- FILTS----
            // 🛠️ Default date
            if (!startDate.HasValue)
            {
                startDate = DateTime.Today;
            }
            if (!endDate.HasValue)
            {
                endDate = new DateTime(DateTime.Today.Year, 12, 31);
            }

            if (searchRequestId.HasValue)
            {
                leaveRequests = leaveRequests.Where(lr => lr.ID == searchRequestId.Value).ToList();
                ViewData["SearchRequestId"] = searchRequestId.Value;
            }
            if (selectedStatuses != null && selectedStatuses.Any())
            {
                leaveRequests = leaveRequests.Where(lr => selectedStatuses.Contains(lr.Status)).ToList();
            }

            if (startDate.HasValue)
            {
                leaveRequests = leaveRequests.Where(lr => lr.StartDate >= startDate.Value).ToList();
            }

            if (endDate.HasValue)
            {
                leaveRequests = leaveRequests.Where(lr => lr.EndDate <= endDate.Value).ToList();
            }
            leaveRequests = sortOrder switch
            {
                "id_desc" => leaveRequests.OrderByDescending(lr => lr.ID).ToList(),
                "employee_asc" => leaveRequests.OrderBy(lr => lr.EmployeeFullName).ToList(),
                "employee_desc" => leaveRequests.OrderByDescending(lr => lr.EmployeeFullName).ToList(),
                "startdate_asc" => leaveRequests.OrderBy(lr => lr.StartDate).ToList(),
                "startdate_desc" => leaveRequests.OrderByDescending(lr => lr.StartDate).ToList(),
                "createdat_desc" => leaveRequests.OrderByDescending(lr => lr.CreatedAt).ToList(),
                "createdat_asc" => leaveRequests.OrderBy(lr => lr.CreatedAt).ToList(),
                _ => leaveRequests.OrderBy(lr => lr.ID).ToList(), 
            };
            ViewBag.SelectedStatuses = selectedStatuses ?? new List<string>();
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            int pageSize = 10;
            int pageIndex = pageNumber ?? 1;

            var pagedList = leaveRequests.ToPagedList(pageIndex, pageSize);

            return View(pagedList);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var leaveRequest = await _mediator.Send(new GetLeaveRequestByIdQuery { Id = id });
            if (leaveRequest == null)
            {
                return NotFound();
            }
          
            return View(leaveRequest);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateLeaveRequestCommand command)
        {
            if (!User.Identity.IsAuthenticated)
                return Forbid();

            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.Users.Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == appUserId);

            if (user?.Employee == null)
            {
                _logger.LogWarning("Nie przypisano Employee do użytkownika {User}", appUserId);
                return Forbid();
            }

            command.EmployeeId = user.Employee.Id;

            try
            {
                await _mediator.Send(command);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View(command);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var command = new UpdateLeaveRequestStatusCommand
            {
                LeaveRequestID = id,
                Status = status
            };
            await _mediator.Send(command);
            return RedirectToAction(nameof(Index));
        }
    }
}
