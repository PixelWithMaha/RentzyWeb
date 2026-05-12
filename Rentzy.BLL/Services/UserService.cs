using Rentzy.DAL.Models;
using Rentzy.DAL.Repositories;
using Rentzy.BLL.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rentzy.BLL.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task AddUserAsync(User user)
        {
            if (user == null || user is NoUser)
            {
                throw new ValidationException("Cannot add null or invalid user");
            }

            await _userRepository.AddUser(user);
            await _userRepository.SaveChanges();
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ValidationException("Email is required");
            }

            var user = await _userRepository.GetUserByEmail(email);

            if (user is NoUser)
            {
                throw new NotFoundException($"User with email '{email}' not found");
            }

            return user;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ValidationException("Invalid user ID");
            }

            var user = await _userRepository.GetUserById(id);

            if (user is NoUser)
            {
                throw new NotFoundException($"User with ID {id} not found");
            }

            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            if (user == null || user is NoUser)
            {
                throw new ValidationException("Cannot update null or invalid user");
            }

            await _userRepository.UpdateUser(user);
            await _userRepository.SaveChanges();
        }

        public async Task DeleteUserAsync(int id)
        {
            if (id <= 0)
            {
                throw new ValidationException("Invalid user ID");
            }

            await _userRepository.DeleteUser(id);
            await _userRepository.SaveChanges();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsers();
        }

        public async Task<List<Tenant>> GetAllTenantsAsync()
        {
            return await _userRepository.GetAllTenants();
        }

        public async Task<List<Landlord>> GetAllLandlordsAsync()
        {
            return await _userRepository.GetAllLandlords();
        }

        public async Task<List<Landlord>> GetVerifiedLandlordsAsync()
        {
            return await _userRepository.GetVerifiedLandlords();
        }

        public async Task<List<User>> SearchUsersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ValidationException("Search term is required");
            }

            return await _userRepository.SearchUsers(searchTerm);
        }
    }
}