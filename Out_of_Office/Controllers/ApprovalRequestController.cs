using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Out_of_Office.Application.Approval_Request.Command.CreateApprovalRequest;
using Out_of_Office.Application.Approval_Request.Command.UpdateApprovalRequestStatus;
using Out_of_Office.Application.Approval_Request.Query.GetAllApprovalRequestQuery;
using Out_of_Office.Application.Approval_Request.Query.GetApprovalRequestByIdQuery;
using Out_of_Office.Domain.Entities;
using X.PagedList;

namespace Out_of_Office.Controllers
{
    public class ApprovalRequestController:Controller
    {
        private readonly IMediator _mediator;

        public ApprovalRequestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? searchRequestId, string sortOrder, string statusFilter, int? pageNumber)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.IdSortParm = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewBag.ApproverSortParm = sortOrder == "approver_asc" ? "approver_desc" : "approver_asc";
            ViewBag.StatusSortParm = sortOrder == "status_asc" ? "status_desc" : "status_asc";

            var approvalRequests = await _mediator.Send(new GetAllApprovalRequestQuery());

            if (searchRequestId.HasValue)
            {
                approvalRequests = approvalRequests.Where(ar => ar.LeaveRequestID == searchRequestId.Value).ToList();
                ViewData["SearchRequestId"] = searchRequestId.Value;
            }
            if (!string.IsNullOrEmpty(statusFilter))
            {
                approvalRequests = approvalRequests
                    .Where(ar => ar.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
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
                await _mediator.Send(command);
                return RedirectToAction(nameof(Index));
            }
            return View("CreateApprovalRequest", command);
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
        public async Task<IActionResult> Reject(int id)
        {
            var updateApprovalStatusCommand = new UpdateApprovalRequestStatusCommand
            {
                ApprovalRequestId = id,
                Status = ApprovalStatus.Rejected,
            };

            await _mediator.Send(updateApprovalStatusCommand);

            return RedirectToAction(nameof(Index));
        }
    }
}
