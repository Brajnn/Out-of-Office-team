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

        public UpdateApprovalRequestStatusCommandHandler(IApprovalRequestRepository approvalRequestRepository, ILeaveRequestRepository leaveRequestRepository, IEmployeeRepository employeeRepository)
        {
            _approvalRequestRepository = approvalRequestRepository;
            _leaveRequestRepository = leaveRequestRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<Unit> Handle(UpdateApprovalRequestStatusCommand request, CancellationToken cancellationToken)
        {
            var approvalRequest = await _approvalRequestRepository.GetApprovalRequestByIdAsync(request.ApprovalRequestId);
            if (approvalRequest == null)
            {
                throw new NotFoundException(nameof(ApprovalRequest), request.ApprovalRequestId);
            }

            var leaveRequest = await _leaveRequestRepository.GetLeaveRequestByIdAsync(approvalRequest.LeaveRequestID);
            if (leaveRequest == null)
            {
                throw new NotFoundException(nameof(LeaveRequest), approvalRequest.LeaveRequestID);
            }

            approvalRequest.StatusChangedAt = DateTime.Now;
            if (request.Status == ApprovalStatus.Approved)
            {
                leaveRequest.Status = LeaveRequest.AbsenceStatus.Approved;
                approvalRequest.Status = ApprovalStatus.Approved;

            }
            else
            {
                leaveRequest.Status = LeaveRequest.AbsenceStatus.Rejected;
                approvalRequest.Status = ApprovalStatus.Rejected;

            }

            await _approvalRequestRepository.UpdateAsync(approvalRequest);
            await _leaveRequestRepository.UpdateAsync(leaveRequest);

            var employee = await _employeeRepository.GetEmployeeByIdAsync(leaveRequest.EmployeeID);
            if (employee != null)
            {
                var approvedLeaveRequests = await _leaveRequestRepository.GetAllLeaveRequestsAsync();
                var totalApprovedDays = approvedLeaveRequests
                    .Where(lr => lr.EmployeeID == leaveRequest.EmployeeID && lr.Status == LeaveRequest.AbsenceStatus.Approved)
                    .Sum(lr => (lr.EndDate - lr.StartDate).Days + 1);

                employee.OutOfOfficeBalance = totalApprovedDays;
                await _employeeRepository.UpdateEmployeeAsync(employee);
            }

            return Unit.Value;
        }
    }
    
}
