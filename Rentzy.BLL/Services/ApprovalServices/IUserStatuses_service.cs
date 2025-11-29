using Rentzy.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.BLL.Services.ApprovalServices
{
    public interface IUserStatuses_service
    {
        Task<UserStatus> GetStatusAsync(int userId);
        Task<bool> IsBlockedAsync(int userId);
        Task BlockUserAsync(int userId);
        Task UnblockUserAsync(int userId);
        Task<bool> CanEditAsync(int userId);
    }
}
