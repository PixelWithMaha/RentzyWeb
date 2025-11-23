using Rentzy.DAL.Models;
using Rentzy.DAL.Repositories;
using Rentzy.BLL.DTOs;
using Microsoft.EntityFrameworkCore;
using Rentzy.BLL.Exceptions;
using System;
using System.Threading.Tasks;

namespace Rentzy.BLL.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<UserDTO> RegisterUserAsync(RegisterDTO dto)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                throw new ArgumentException("Email and password are required");
            }

            if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
            {
                throw new ArgumentException("First name and last name are required");
            }

            // Check if email already exists
            var existingUser = await _userRepository.GetUserByEmail(dto.Email);
            if (existingUser is not NoUser)
            {
                throw new InvalidOperationException("Email already exists");
            }

            // Create the appropriate user type
            User newUser = dto.UserType.ToLower() switch
            {
                "tenant" => new Tenant(),
                "landlord" => new Landlord { IsVerified = false },
                "admin" => new Admin { Role = "Admin" },
                _ => throw new ArgumentException("Invalid user type. Must be 'tenant', 'landlord', or 'admin'")
            };

            // Set common properties
            newUser.Email = dto.Email;
            newUser.FirstName = dto.FirstName;
            newUser.LastName = dto.LastName;
            newUser.Phone = dto.Phone;
            newUser.PasswordHash = HashPassword(dto.Password);
            newUser.CreatedAt = DateTime.UtcNow;

            try
            {
                await _userRepository.AddUser(newUser);
                await _userRepository.SaveChanges();
                return MapToDto(newUser);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Unable to complete registration. Please try again later.", ex);
            }
        }

        public async Task<UserDTO> LoginAsync(LoginDTO dto)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                throw new ValidationException("Email and password are required");
            }

            // Get user by email
            var user = await _userRepository.GetUserByEmail(dto.Email);

            if (user is NoUser)
            {
                // Don't reveal if email exists - security best practice
                throw new AuthenticationException("Invalid email or password");
            }

            // Verify password
            if (!VerifyPassword(dto.Password, user.PasswordHash))
            {
                // Wrong password
                throw new AuthenticationException("Invalid email or password");
            }

            return MapToDto(user);
        }

        public async Task<UserDTO> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
            {
                throw new ArgumentException("First name and last name are required");
            }

            // Get user
            var user = await _userRepository.GetUserById(userId);

            if (user is NoUser)
            {
                throw new InvalidOperationException("User not found");
            }

            // Update properties
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Phone = dto.Phone;

            try
            {
                await _userRepository.UpdateUser(user);
                await _userRepository.SaveChanges();
                return MapToDto(user);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Unable to update profile. Please try again later.", ex);
            }
        }

        public async Task<bool> IsLandlordVerifiedAsync(int userId)
        {
            var user = await _userRepository.GetUserById(userId);

            if (user is NoUser)
            {
                throw new InvalidOperationException("User not found");
            }

            if (user is not Landlord landlord)
            {
                throw new InvalidOperationException("User is not a landlord");
            }

            return landlord.IsVerified;
        }

        public async Task<UserDTO> GetUserByIdAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Invalid user ID");
            }

            var user = await _userRepository.GetUserById(userId);

            if (user is NoUser)
            {
                throw new InvalidOperationException("User not found");
            }

            return MapToDto(user);
        }

        public async Task<UserDTO> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email is required");
            }

            var user = await _userRepository.GetUserByEmail(email);

            if (user is NoUser)
            {
                throw new InvalidOperationException("User not found");
            }

            return MapToDto(user);
        }

        private UserDTO MapToDto(User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                UserType = user.GetType().Name,
                CreatedAt = user.CreatedAt
            };
        }

        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, 12);
        }

        private static bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email is required");
            }

            var user = await _userRepository.GetUserByEmail(email);

            if (user is NoUser)
            {
                // Don't reveal that user doesn't exist for security
                // Still return a token format but it won't work
                return Guid.NewGuid().ToString();
            }

            // Generate unique token
            var token = Guid.NewGuid().ToString();

            // Set token and expiry (1 hour from now)
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            try
            {
                await _userRepository.UpdateUser(user);
                await _userRepository.SaveChanges();
                return token;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Unable to generate reset token. Please try again later.", ex);
            }
        }

        public async Task<UserDTO> ResetPasswordAsync(resetPasswordDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.Email))
            {
                throw new ArgumentException("Token and email are required");
            }

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                throw new ArgumentException("New password is required");
            }

            var user = await _userRepository.GetUserByEmail(dto.Email);

            if (user is NoUser)
            {
                throw new InvalidOperationException("Invalid reset token or email");
            }

            // Verify token matches
            if (user.PasswordResetToken != dto.Token)
            {
                throw new InvalidOperationException("Invalid reset token");
            }

            // Verify token hasn't expired
            if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Reset token has expired. Please request a new one.");
            }

            // Update password
            user.PasswordHash = HashPassword(dto.NewPassword);

            // Clear reset token
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            try
            {
                await _userRepository.UpdateUser(user);
                await _userRepository.SaveChanges();
                return MapToDto(user);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Unable to reset password. Please try again later.", ex);
            }
        }

        public async Task<UserDTO> ChangePasswordAsync(ChangePasswordDTO dto)
        {
            var user = await _userRepository.GetUserById(dto.UserId);

            if (user is NoUser)
            {
                throw new NotFoundException("User not found");
            }

            // Verify current password is correct
            if (!VerifyPassword(dto.CurrentPassword, user.PasswordHash))
            {
                throw new ValidationException("Current password is incorrect");
            }

            // Don't allow same password
            if (dto.CurrentPassword == dto.NewPassword)
            {
                throw new ValidationException("New password must be different from current password");
            }

            // Update password
            user.PasswordHash = HashPassword(dto.NewPassword);

            await _userRepository.UpdateUser(user);
            await _userRepository.SaveChanges();

            return MapToDto(user);
        }
    }
}