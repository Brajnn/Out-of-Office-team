using Microsoft.EntityFrameworkCore;
using Out_of_Office.Application.Approval_Request;
using Out_of_Office.Domain.Entities;
using Out_of_Office.Domain.Interfaces;
using Out_of_Office.Infrastructure.Presistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Infrastructure.Repositories
{
    public class ApprovalRequestRepository:IApprovalRequestRepository
    {
        private readonly Out_of_OfficeDbContext _dbContext;

        public ApprovalRequestRepository(Out_of_OfficeDbContext context)
        {
            _dbContext = context;
        }

        public async Task<IEnumerable<ApprovalRequest>> GetAllApprovalRequestsAsync()
        {
            return await _dbContext.ApprovalRequest
                .Include(ar => ar.Approver)
                .Include(ar => ar.LeaveRequest)
                .ThenInclude(lr => lr.Employee)
                .ToListAsync();
        }
        public async Task<ApprovalRequest> GetApprovalRequestByIdAsync(int id)
        {
            return await _dbContext.ApprovalRequest
                        .Include(ar => ar.Approver)                       
                        .Include(ar => ar.LeaveRequest)
                        .ThenInclude(lr => lr.Employee)
                        .FirstOrDefaultAsync(ar => ar.ID == id);
        }
        public async Task<ApprovalRequest> GetApprovalRequestByLeaveRequestIdAsync(int leaveRequestId)
        {
            return await _dbContext.ApprovalRequest
                                 .FirstOrDefaultAsync(ar => ar.LeaveRequestID == leaveRequestId);
        }

        public async Task AddApprovalRequestAsync(ApprovalRequest approvalRequest)
        {
            _dbContext.ApprovalRequest.Add(approvalRequest);
            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdateAsync(ApprovalRequest approvalRequest)
        {
            _dbContext.ApprovalRequest.Update(approvalRequest);
            await _dbContext.SaveChangesAsync();
        }
    }
}
