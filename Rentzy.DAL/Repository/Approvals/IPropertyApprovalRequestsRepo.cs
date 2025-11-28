using Rentzy.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.DAL.Repository.Approvals
{
    public interface IPropertyApprovalRequestsRepo
    {
        Task CreateAsync(PropertyApprovalRequest request);
        Task<List<PropertyApprovalRequest>> GetAllAsync();
        Task<PropertyApprovalRequest> GetByID(int id);

        Task<List<PropertyApprovalRequest>> GetApprovalByStatusAsync(int statusId);
        Task<List<PropertyApprovalRequest>> GetPendingAsync();
        Task<PropertyApprovalRequest> GetByPropertyIdAsync(int propertyId);

        Task UpdateStatusAsync(PropertyApprovalRequest approval);
        Task SaveChangesAsync();


    }
}
