using Microsoft.EntityFrameworkCore;
using Rentzy.DAL.Context;
using Rentzy.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rentzy.DAL.Models.LandlordApproval;

namespace Rentzy.DAL.Repository.Approvals
{
    public class LandlordApprovalRepository : ILandlordApprovalRepository
    {
        private readonly RentzyDBContext _context;

        public LandlordApprovalRepository(RentzyDBContext context)
        {
            _context = context;
        }

        public async Task AddAsync(LandlordApproval entity)
        {
            await _context.LandlordApprovals.AddAsync(entity);
        }

        public async Task<List<LandlordApproval>> GetAllAsync()
        {
            return await _context.LandlordApprovals
                .Include(x => x.Landlord)
                .Include(x => x.ApprovalStatus)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.SubmittedAt)
                .ToListAsync();
        }

        public async Task<LandlordApproval?> GetByIdAsync(int id)
        {
            return await _context.LandlordApprovals
                .Include(x => x.Landlord)
                .Include(x => x.ApprovalStatus)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<LandlordApproval?> GetByLandlordIdAsync(int landlordId)
        {
            return await _context.LandlordApprovals
                .Include(x => x.Landlord)
                .Include(x => x.ApprovalStatus)
                .Where(x => x.LandlordId == landlordId && !x.IsDeleted)
                .OrderByDescending(x => x.SubmittedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<LandlordApproval>> GetByStatusAsync(int approvalStatusId)
        {
            return await _context.LandlordApprovals
                .Include(x => x.Landlord)
                .Include(x => x.ApprovalStatus)
                .Where(x => x.ApprovalStatusId == approvalStatusId && !x.IsDeleted)
                .OrderByDescending(x => x.SubmittedAt)
                .ToListAsync();
        }

        public async Task<List<LandlordApproval>> GetPendingAsync()
        {
            const int pendingId = ApprovalStatusConstants.Pending; // see constants below
            return await GetByStatusAsync(pendingId);
        }

        public async Task UpdateAsync(LandlordApproval entity)
        {
            _context.LandlordApprovals.Update(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
