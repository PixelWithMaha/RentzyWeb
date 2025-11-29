using Azure.Core;
using Rentzy.DAL.Context;
using Rentzy.DAL.Models;
using Rentzy.DAL.Repository.Approvals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.BLL.Services.ApprovalServices
{
    public class PropertyApprovalRequestService: IPropertyApprovalRequestService
    {
        private readonly IPropertyApprovalRequestsRepo _repo;
        private readonly RentzyDBContext _context;

        public PropertyApprovalRequestService(RentzyDBContext context, IPropertyApprovalRequestsRepo repo)
        {
            _context = context;
            _repo = repo;
        }

        public async Task<List<PropertyApprovalRequest>> GetAllApprovalsAsync()
        {
            return await _repo.GetAllAsync();
        }
        public async Task<List<PropertyApprovalRequest>> GetApprovalsByStatusAsync(int statusId)
        {
            return await _repo.GetApprovalByStatusAsync(statusId);
        }
        public async Task<List<PropertyApprovalRequest>> GetPendingApprovalsAsync()
        {
            return await _repo.GetPendingAsync();
        }
        public async Task<PropertyApprovalRequest?> GetByIdAsync(int id)
        {
            return await _repo.GetByID(id);
        }

        public async Task SubmitApprovalRequestAsync(PropertyApprovalRequest request)
        {
            if (request.StatusId == 0)
                request.StatusId = ApprovalStatusConstants.Pending;
             await _repo.CreateAsync(request);
             _repo.SaveChangesAsync();
        }
        public async Task ApproveAsync(int approvalId, int adminUserId, string? notes = null)
        {
            var item = await _repo.GetByID(approvalId);
            if (item == null) throw new InvalidOperationException("Approval request not found.");

            item.StatusId = ApprovalStatusConstants.Approved;
            item.ReviewedAt = DateTime.UtcNow;
            item.AdminId = adminUserId;
            item.Comments = notes;
         
            await _repo.UpdateStatusAsync(item);
            await _repo.SaveChangesAsync();
        }
        public async Task RejectAsync(int approvalId, int adminUserId, string? notes = null)
        {
            var item = await _repo.GetByID(approvalId);
            if (item == null) throw new InvalidOperationException("Approval request not found.");

            item.StatusId = ApprovalStatusConstants.Rejected;
            item.ReviewedAt = DateTime.UtcNow;
            item.AdminId = adminUserId;
            item.Comments = notes;

           
            await _repo.UpdateStatusAsync(item);
            await _repo.SaveChangesAsync();
        }
        public async Task CancelAsync(int approvalId, int adminUserId, string? notes = null)
        {
            var item = await _repo.GetByID(approvalId);
            if (item == null) throw new InvalidOperationException("Approval request not found.");

            item.StatusId = ApprovalStatusConstants.Cancelled;
            item.ReviewedAt = DateTime.UtcNow;
            item.AdminId = adminUserId;
            item.Comments = notes;

            await _repo.UpdateStatusAsync(item);
            await _repo.SaveChangesAsync();
        }
    }
}
