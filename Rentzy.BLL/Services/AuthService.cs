using Microsoft.EntityFrameworkCore;
using Rentzy.BLL.DTOs;
using Rentzy.BLL.Exceptions;
using Rentzy.BLL.Factory;
using Rentzy.DAL.Models;
using Rentzy.DAL.Repositories;
using Rentzy.DAL.Repository.Approvals;
using System;
using System.Threading.Tasks;
using Rentzy.BLL.Services.ApprovalServices;

namespace Rentzy.BLL.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILandlordApprovalRepository _landlordApprovalRepository;
        private readonly IUserStatuses_service _UserStatusService;

        private readonly EmailService _emailService;

        public AuthService(IUserRepository userRepository, ILandlordApprovalRepository _repo, EmailService emailService, IUserStatuses_service service) 
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _landlordApprovalRepository = _repo ?? throw new ArgumentNullException(nameof(_repo));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _UserStatusService = service ?? throw new ArgumentNullException(nameof(service));
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

            var newUser = UserFactory.CreateUser(dto.UserType);

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

                UserDTO user = MapToDto(newUser);

                if (newUser.Role == "Landlord")
                {
                    var approval = new LandlordApproval
                    {
                        LandlordId = user.Id,
                        SubmittedAt = DateTime.UtcNow,
                        DocumentUrl = "N/A",         // Ya jo bhi file ho
                        ApprovalStatusId = 1,  // Set Pending
                        IsDeleted = false,
                    };
                    await _landlordApprovalRepository.AddAsync(approval);
                    await _landlordApprovalRepository.SaveChangesAsync();
                }

                await _UserStatusService.AddInUserStatus(newUser.Id);

                return user;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"CRITICAL DB ERROR IN REGISTRATION: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"INNER EXCEPTION: {ex.InnerException.Message}");
                }
                throw new InvalidOperationException("Unable to complete registration. Please try again later.", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UNEXPECTED ERROR IN REGISTRATION: {ex.GetType().Name} - {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"INNER EXCEPTION: {ex.InnerException.Message}");
                }
                throw;
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
                throw new NotFoundException("User not found");
            }

            if (user is not Landlord landlord)
            {
                throw new BusinessException("User is not a landlord");
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
                CreatedAt = user.CreatedAt,
                Role = user.Role
            };
        }

        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, 8);
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
                // For security, don't reveal if email doesn't exist
                // But still send email attempt for logging
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

                // ✅ BUILD THE RESET URL
                // Note: In production, get the actual domain from configuration
                var resetUrl = $"https://localhost:44359/Account/ResetPassword?token={token}&email={email}";

                // ✅ SEND EMAIL
                await _emailService.SendPasswordResetEmailAsync(email, token, resetUrl);

                return token;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Unable to generate reset token. Please try again later.", ex);
            }
        }

        // Add this NEW method for welcome email
        public async Task SendWelcomeEmailAsync(string email, string userName)
        {
            try
            {
                await _emailService.SendWelcomeEmailAsync(email, userName);
            }
            catch (Exception ex)
            {
                // Log but don't fail registration if email fails
                Console.WriteLine($"Failed to send welcome email: {ex.Message}");
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