using Rentzy.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rentzy.DAL.Models.LandlordApproval;

namespace Rentzy.DAL.Repository.Approvals
{
    public interface ILandlordApprovalRepository
    {
        Task<List<LandlordApproval>> GetAllAsync();
        Task<List<LandlordApproval>> GetByStatusAsync(int approvalStatusId);
        Task<List<LandlordApproval>> GetPendingAsync(); // convenience
        Task<LandlordApproval?> GetByIdAsync(int id);
        Task<LandlordApproval?> GetByLandlordIdAsync(int landlordId);
        Task AddAsync(LandlordApproval entity);
        Task UpdateAsync(LandlordApproval entity);
        Task SaveChangesAsync();
    }
   
}
