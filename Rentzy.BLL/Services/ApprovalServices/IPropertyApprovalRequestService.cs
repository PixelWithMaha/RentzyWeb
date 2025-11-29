using Rentzy.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.BLL.Services.ApprovalServices
{
    public interface IPropertyApprovalRequestService
    {
        Task<List<PropertyApprovalRequest>> GetAllApprovalsAsync();
        Task<List<PropertyApprovalRequest>> GetApprovalsByStatusAsync(int statusId);
        Task<List<PropertyApprovalRequest>> GetPendingApprovalsAsync();
        Task<PropertyApprovalRequest?> GetByIdAsync(int id);
        Task SubmitApprovalRequestAsync(PropertyApprovalRequest request); // create new request
        Task ApproveAsync(int approvalId, int adminUserId, string? notes = null);
        Task RejectAsync(int approvalId, int adminUserId, string? notes = null);
        Task CancelAsync(int approvalId, int adminUserId, string? notes = null);
    }
}
