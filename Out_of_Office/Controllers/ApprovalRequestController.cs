using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Out_of_Office.Application.Approval_Request.Command.CreateApprovalRequest;
using Out_of_Office.Application.Approval_Request.Command.UpdateApprovalRequestStatus;
using Out_of_Office.Application.Approval_Request.Query.GetAllApprovalRequestQuery;
using Out_of_Office.Application.Approval_Request.Query.GetApprovalRequestByIdQuery;
using Out_of_Office.Domain.Entities;
using Out_of_Office.Infrastructure.Identity;
using System.Security.Claims;
using X.PagedList;

namespace Out_of_Office.Controllers
{
    public class ApprovalRequestController:Controller
    {
        private readonly IMediator _mediator;
        private readonly UserManager<ApplicationUser> _userManager;
        public ApprovalRequestController(IMediator mediator, UserManager<ApplicationUser> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? searchRequestId, string sortOrder, string statusFilter, int? pageNumber, List<string> selectedStatuses, string employeeName, DateTime? startDate, DateTime? endDate)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.IdSortParm = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewBag.ApproverSortParm = sortOrder == "approver_asc" ? "approver_desc" : "approver_asc";
            ViewBag.StatusSortParm = sortOrder == "status_asc" ? "status_desc" : "status_asc";


            var user = await _userManager.GetUserAsync(User);
            if (user?.EmployeeId == null || user.EmployeeId == 0)
                return Unauthorized();

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            var userId = user.EmployeeId;
            var approvalRequests = await _mediator.Send(new GetAllApprovalRequestQuery
            {
                UserId = user.EmployeeId.Value,
                UserRole = role
            });
            if (!startDate.HasValue)
                startDate = DateTime.Today;
            if (!endDate.HasValue)
                endDate = new DateTime(DateTime.Today.Year, 12, 31);

            // Request number search
            if (searchRequestId.HasValue)
            {
                approvalRequests = approvalRequests.Where(ar => ar.LeaveRequestID == searchRequestId.Value).ToList();
                ViewData["SearchRequestId"] = searchRequestId.Value;
            }
            // Employee name search
            if (!string.IsNullOrEmpty(employeeName))
            {
                approvalRequests = approvalRequests.Where(ar => ar.EmployeeFullName.Contains(employeeName, StringComparison.OrdinalIgnoreCase)).ToList();
                ViewData["EmployeeName"] = employeeName;
            }
            // Filter by Selected Statuses
            if (selectedStatuses != null && selectedStatuses.Any())
            {
                approvalRequests = approvalRequests.Where(ar => selectedStatuses.Contains(ar.Status)).ToList();
            }
            // 📅 Filter by Received Date
            approvalRequests = approvalRequests.Where(ar =>
                ar.LeaveRequestSubmittedAt.HasValue &&
                ar.LeaveRequestSubmittedAt.Value.Date >= startDate.Value.Date &&
                ar.LeaveRequestSubmittedAt.Value.Date <= endDate.Value.Date
            ).ToList();
           
           
            approvalRequests = sortOrder switch
            {
                "id_desc" => approvalRequests.OrderByDescending(ar => ar.ID).ToList(),
                "approver_asc" => approvalRequests.OrderBy(ar => ar.ApproverFullName).ToList(),
                "approver_desc" => approvalRequests.OrderByDescending(ar => ar.ApproverFullName).ToList(),
                "status_asc" => approvalRequests.OrderBy(ar => ar.Status).ToList(),
                "status_desc" => approvalRequests.OrderByDescending(ar => ar.Status).ToList(),
                _ => approvalRequests.OrderBy(ar => ar.ID).ToList(),
            };
            ViewBag.Statuses = new SelectList(Enum.GetNames(typeof(ApprovalStatus)));
            ViewBag.SelectedStatuses = selectedStatuses ?? new List<string>();
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            int pageSize = 10;
            int pageIndex = pageNumber ?? 1;

            var pagedList = approvalRequests.ToPagedList(pageIndex, pageSize);
            return View(pagedList);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var approvalRequest = await _mediator.Send(new GetApprovalRequestByIdQuery(id));
            if (approvalRequest == null)
            {
                return NotFound();
            }
            return View(approvalRequest);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("CreateApprovalRequest");
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateApprovalRequestCommand command)
        {
            if (ModelState.IsValid)
            {
                return View("CreateApprovalRequest", command);
            }
            try
            {
                await _mediator.Send(command);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                
                return View("ErrorMessage", $"Failed to send application: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var updateApprovalStatusCommand = new UpdateApprovalRequestStatusCommand()
            {
                ApprovalRequestId = id,
                Status = ApprovalStatus.Approved
            };

            await _mediator.Send(updateApprovalStatusCommand);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id, string comment)
        {
            var updateApprovalStatusCommand = new UpdateApprovalRequestStatusCommand
            {
                ApprovalRequestId = id,
                Status = ApprovalStatus.Rejected,
                DecisionComment = comment
            };

            await _mediator.Send(updateApprovalStatusCommand);

            return RedirectToAction(nameof(Index));
        }

    }
}
