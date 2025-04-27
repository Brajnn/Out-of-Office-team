using MediatR;
using Out_of_Office.Application.Common.Exceptions;
using Out_of_Office.Domain.Entities;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Approval_Request.Command.UpdateApprovalRequestStatus
{
    public class UpdateApprovalRequestStatusCommandHandler:IRequestHandler<UpdateApprovalRequestStatusCommand>
    {
        private readonly IApprovalRequestRepository _approvalRequestRepository;
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IWorkCalendarRepository _calendarRepository;

        public UpdateApprovalRequestStatusCommandHandler(IApprovalRequestRepository approvalRequestRepository, ILeaveRequestRepository leaveRequestRepository, IEmployeeRepository employeeRepository,IWorkCalendarRepository calendarRepository)
        {
            _approvalRequestRepository = approvalRequestRepository;
            _leaveRequestRepository = leaveRequestRepository;
            _employeeRepository = employeeRepository;
            _calendarRepository = calendarRepository;
        }

        public async Task<Unit> Handle(UpdateApprovalRequestStatusCommand request, CancellationToken cancellationToken)
        {
            var approvalRequest = await _approvalRequestRepository.GetApprovalRequestByIdAsync(request.ApprovalRequestId)
               ?? throw new NotFoundException(nameof(ApprovalRequest), request.ApprovalRequestId);

            var leaveRequest = await _leaveRequestRepository.GetLeaveRequestByIdAsync(approvalRequest.LeaveRequestID)
                ?? throw new NotFoundException(nameof(LeaveRequest), approvalRequest.LeaveRequestID);

            var employee = await _employeeRepository.GetEmployeeByIdAsync(leaveRequest.EmployeeID)
                ?? throw new NotFoundException(nameof(Employee), leaveRequest.EmployeeID);

            approvalRequest.StatusChangedAt = DateTime.Now;

            if (approvalRequest.Status == ApprovalStatus.New && leaveRequest.StartDate.Date < DateTime.Today)
            {
                leaveRequest.Status = LeaveRequest.AbsenceStatus.Rejected;
                approvalRequest.Status = ApprovalStatus.Rejected;
                approvalRequest.DecisionComment = "Leave request was automatically rejected due to overdue approval.";
            }
            else if (request.Status == ApprovalStatus.Approved)
            {
                var allRequests = await _leaveRequestRepository.GetAllLeaveRequestsAsync();
                var overlaps = allRequests.Any(lr =>
                    lr.EmployeeID == leaveRequest.EmployeeID &&
                    lr.Status == LeaveRequest.AbsenceStatus.Approved &&
                    lr.ID != leaveRequest.ID &&
                    lr.StartDate <= leaveRequest.EndDate &&
                    lr.EndDate >= leaveRequest.StartDate);
                if (overlaps)
                    throw new InvalidOperationException("This leave overlaps with an already approved leave request.");

                leaveRequest.Status = LeaveRequest.AbsenceStatus.Approved;
                approvalRequest.Status = ApprovalStatus.Approved;
            }
            else if (request.Status == ApprovalStatus.Rejected)
            {
                leaveRequest.Status = LeaveRequest.AbsenceStatus.Rejected;
                approvalRequest.Status = ApprovalStatus.Rejected; 
                approvalRequest.DecisionComment = request.DecisionComment;


            }

            await _approvalRequestRepository.UpdateAsync(approvalRequest);
            await _leaveRequestRepository.UpdateAsync(leaveRequest);

            return Unit.Value;
        }
    }
    
}
