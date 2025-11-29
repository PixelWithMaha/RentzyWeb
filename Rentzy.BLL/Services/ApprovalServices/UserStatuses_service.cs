using Rentzy.DAL.Models;
using Rentzy.DAL.Repository.Approvals;
using Rentzy.DAL.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.BLL.Services.ApprovalServices
{
    public class UserStatuses_service: IUserStatuses_service
    {
        private readonly IUserStatuses_Repo _repo;

        public UserStatuses_service(IUserStatuses_Repo repo)
        {
            _repo = repo;
        }

        public async Task<UserStatus> GetStatusAsync(int userId)
        {
            var status = await _repo.GetByUserIdAsync(userId);

            // Agar status exist nahi karta (new user)
            if (status == null)
            {
                status = new UserStatus
                {
                    UserId = userId,
                    IsActive = true,
                    IsDeleted = false
                };

                await _repo.AddAsync(status);
                await _repo.SaveChangesAsync();
            }

            return status;
        }

        public async Task<bool> IsBlockedAsync(int userId)
        {
            var status = await GetStatusAsync(userId);
            return status.IsDeleted == true;
        }

        public async Task BlockUserAsync(int userId)
        {
            await _repo.BlockUserAsync(userId);
            await _repo.SaveChangesAsync();
        }

        public async Task UnblockUserAsync(int userId)
        {
            await _repo.UnblockUserAsync(userId);
            await _repo.SaveChangesAsync();
        }

        public async Task<bool> CanEditAsync(int userId)
        {
            var status = await GetStatusAsync(userId);

            // RULE: Sirf unblocked users editable hun
            return (status.IsDeleted == false && status.IsActive == true);
        }
    }
}
