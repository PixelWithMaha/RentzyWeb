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
    public class UserStatuses_Repo: IUserStatuses_Repo
    {
        private readonly RentzyDBContext _context;

        public UserStatuses_Repo(RentzyDBContext context)
        {
            _context = context;
        }

        public async Task<UserStatus> GetByUserIdAsync(int userId)
        {
            return await _context.UserStatuses
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task AddAsync(UserStatus status)
        {
            await _context.UserStatuses.AddAsync(status);
        }

        public async Task BlockUserAsync(int userId)
        {
            var status = await GetByUserIdAsync(userId);

            if (status == null)
            {
                status = new UserStatus
                {
                    UserId = userId,
                    IsActive = false,
                    IsDeleted = true
                };
                await AddAsync(status);
            }
            else
            {
                status.IsActive = false;
                status.IsDeleted = true;
            }
        }

        public async Task UnblockUserAsync(int userId)
        {
            var status = await GetByUserIdAsync(userId);

            if (status == null)
            {
                status = new UserStatus
                {
                    UserId = userId,
                    IsActive = true,
                    IsDeleted = false
                };
                await AddAsync(status);
            }
            else
            {
                status.IsActive = true;
                status.IsDeleted = false;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
