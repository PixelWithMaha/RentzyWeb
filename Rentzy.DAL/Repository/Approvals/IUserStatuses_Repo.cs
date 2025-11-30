using Rentzy.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.DAL.Repository.Approvals
{
    public interface IUserStatuses_Repo
    {
        Task<UserStatus> GetByUserIdAsync(int userId);
        Task AddAsync(UserStatus status);
        Task BlockUserAsync(int userId);
        Task UnblockUserAsync(int userId);
        Task SaveChangesAsync();
    }
}
