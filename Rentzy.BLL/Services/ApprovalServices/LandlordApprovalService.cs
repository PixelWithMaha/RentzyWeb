using Rentzy.DAL.Context;
using Rentzy.DAL.Models;
using Rentzy.DAL.Repository.Approvals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rentzy.DAL.Models.LandlordApproval;

namespace Rentzy.BLL.Services.ApprovalServices
{
    public class LandlordApprovalService : ILandlordApprovalService
    {
        private readonly ILandlordApprovalRepository _repo;
        private readonly RentzyDBContext _context; // if you need to update Landlord entity (IsVerified) etc.

        public LandlordApprovalService(ILandlordApprovalRepository repo, RentzyDBContext context)
        {
            _repo = repo;
            _context = context;
        }

        public async Task<List<LandlordApproval>> GetAllApprovalsAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<List<LandlordApproval>> GetApprovalsByStatusAsync(int statusId)
        {
            return await _repo.GetByStatusAsync(statusId);
        }

        public async Task<List<LandlordApproval>> GetPendingApprovalsAsync()
        {
            return await _repo.GetPendingAsync();
        }

        public async Task<LandlordApproval?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task SubmitApprovalRequestAsync(LandlordApproval request)
        {
            // Keep the initial status as Pending if not provided
            if (request.ApprovalStatusId == 0)
                request.ApprovalStatusId = ApprovalStatusConstants.Pending;

            await _repo.AddAsync(request);
            await _repo.SaveChangesAsync();
        }

        public async Task ApproveAsync(int approvalId, int adminUserId, string? notes = null)
        {
            var item = await _repo.GetByIdAsync(approvalId);
            if (item == null) throw new InvalidOperationException("Approval request not found.");

            item.ApprovalStatusId = ApprovalStatusConstants.Approved;
            item.ReviewedAt = DateTime.UtcNow;
            item.ReviewedByAdminId = adminUserId;
            item.AdminNotes = notes;

            // Mark landlord as verified (if you want to keep that logic)
            if (item.Landlord != null)
            {
                item.Landlord.IsVerified = true;
                _context.Users.Update(item.Landlord);
            }

            await _repo.UpdateAsync(item);
            await _repo.SaveChangesAsync();
        }

        public async Task RejectAsync(int approvalId, int adminUserId, string? notes = null)
        {
            var item = await _repo.GetByIdAsync(approvalId);
            if (item == null) throw new InvalidOperationException("Approval request not found.");

            item.ApprovalStatusId = ApprovalStatusConstants.Rejected;
            item.ReviewedAt = DateTime.UtcNow;
            item.ReviewedByAdminId = adminUserId;
            item.AdminNotes = notes;

            // Optionally keep landlord.IsVerified false

            await _repo.UpdateAsync(item);
            await _repo.SaveChangesAsync();
        }

        public async Task CancelAsync(int approvalId, int adminUserId, string? notes = null)
        {
            var item = await _repo.GetByIdAsync(approvalId);
            if (item == null) throw new InvalidOperationException("Approval request not found.");

            item.ApprovalStatusId = ApprovalStatusConstants.Cancelled;
            item.ReviewedAt = DateTime.UtcNow;
            item.ReviewedByAdminId = adminUserId;
            item.AdminNotes = notes;

            await _repo.UpdateAsync(item);
            await _repo.SaveChangesAsync();
        }
    }
  
}
