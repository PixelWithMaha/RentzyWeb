using Microsoft.EntityFrameworkCore;
using Rentzy.DAL.Context;
using Rentzy.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.DAL.Repository.Approvals
{
    public class PropertyApprovalRequestsRepo: IPropertyApprovalRequestsRepo
    {
        RentzyDBContext _context;
        public PropertyApprovalRequestsRepo(RentzyDBContext context) 
        {
            _context = context;
        }
        public async Task CreateAsync(PropertyApprovalRequest request)
        {
           await _context.PropertyApprovalRequests.AddAsync(request);
           
        }

        public async Task<PropertyApprovalRequest> GetByID(int id)
        {
            return await _context.PropertyApprovalRequests
                .Include(x => x.property)
                        .ThenInclude(x => x.Landlord)
                .Include(x => x.Status)
                .Include(x => x.Admin)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<PropertyApprovalRequest>> GetAllAsync()
        {
            return await _context.PropertyApprovalRequests
                .Include(x => x.property)
                .Include(x => x.Status)
                .OrderByDescending(x => x.RequestedAt)
                .ToListAsync();
        }

        public async Task<List<PropertyApprovalRequest>> GetApprovalByStatusAsync(int statusID)
        {
            return await _context.PropertyApprovalRequests
                .Include(x => x.property)
                .Include(x => x.Status)
                .Where(x => x.StatusId == statusID)
                .OrderByDescending(x => x.RequestedAt)
                .ToListAsync();
        }

        public async Task<List<PropertyApprovalRequest>> GetPendingAsync()
        {
            const int pendingId = ApprovalStatusConstants.Pending;
            return await _context.PropertyApprovalRequests
                .Include(x => x.property)
                .Include(x => x.Status)
                .Where(x => x.StatusId == pendingId)
                .OrderByDescending(x => x.RequestedAt)
                .ToListAsync();
        }

        public async Task<PropertyApprovalRequest> GetByPropertyIdAsync(int propertyId)
        {
                return await _context.PropertyApprovalRequests
                .Include(x => x.property)
                .Include(x => x.Status)
                .FirstOrDefaultAsync(x => x.PropertyId == propertyId);
        }

        public async Task UpdateStatusAsync(PropertyApprovalRequest approval)
        {
             _context.PropertyApprovalRequests.Update(approval);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
