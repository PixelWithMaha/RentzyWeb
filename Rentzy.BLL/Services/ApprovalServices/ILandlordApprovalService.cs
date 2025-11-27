using Rentzy.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rentzy.DAL.Models.LandlordApproval;

namespace Rentzy.BLL.Services.ApprovalServices
{
    public interface ILandlordApprovalService
    {
        Task<List<LandlordApproval>> GetAllApprovalsAsync();
        Task<List<LandlordApproval>> GetApprovalsByStatusAsync(int statusId);
        Task<List<LandlordApproval>> GetPendingApprovalsAsync();
        Task<LandlordApproval?> GetByIdAsync(int id);
        Task SubmitApprovalRequestAsync(LandlordApproval request); // create new request
        Task ApproveAsync(int approvalId, int adminUserId, string? notes = null);
        Task RejectAsync(int approvalId, int adminUserId, string? notes = null);
        Task CancelAsync(int approvalId, int adminUserId, string? notes = null);
    }
}
