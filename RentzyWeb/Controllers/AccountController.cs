using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Rentzy.BLL.DTOs;
using Rentzy.BLL.Exceptions;
using Rentzy.BLL.Services;
using Rentzy.BLL.Services.ApprovalServices;
using Rentzy.DAL.Models;
using Rentzy.Web.Models;
using System;
using System.Dynamic;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Rentzy.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly IUserStatuses_service _statusService;

        public AccountController(AuthService authService, IUserStatuses_service service)
        {
            _authService = authService;
            _statusService = service;
        }

        // ✅ REGISTER - Use ViewModel
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model) // ✅ ViewModel
        {
            if (!ModelState.IsValid)
            {
                return View(model); 
            }

            try
            {
                // Convert ViewModel → DTO
                var dto = new RegisterDTO
                {
                    Email = model.Email,
                    Password = model.Password,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Phone = model.Phone,
                    UserType = model.UserType
                };

                var userDto = await _authService.RegisterUserAsync(dto);
                

                _ = Task.Run(() =>
                 _authService.SendWelcomeEmailAsync(userDto.Email, userDto.FullName)
                    );

                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model); 
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model) 
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var dto = new LoginDTO
                {
                    Email = model.Email,
                    Password = model.Password
                };

                var userDto = await _authService.LoginAsync(dto);

                HttpContext.Session.SetInt32("UserId", userDto.Id);
                HttpContext.Session.SetString("UserName", userDto.FullName);
                HttpContext.Session.SetString("UserType", userDto.UserType);
                HttpContext.Session.SetString("UserEmail", userDto.Email);

                TempData["SuccessMessage"] = $"Welcome back, {userDto.FullName}!";

                if( userDto.UserType == "Admin")
                    HttpContext.Session.SetString("Id",userDto.Id.ToString());

                if( await _statusService.IsBlockedAsync(userDto.Id))
                {
                    throw (new ValidationException("Your account is currently blocked by Admin!"));
                }

                return userDto.UserType switch
                {
                    "Admin" => RedirectToAction("Dashboard", "Admin"),
                    "Landlord" => RedirectToAction("Dashboard", "Landlord"), 
                    "Tenant" => RedirectToAction("Dashboard", "Tenant"),
                    _ => RedirectToAction("Index", "Home")
                };
            }
            catch (AuthenticationException ex)
            {
                ModelState.AddModelError("",ex.Message.ToString() );
                return View(model);
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please login to view your profile.";
                return RedirectToAction("Login");
            }

            try
            {
                var userDto = await _authService.GetUserByIdAsync(userId.Value);
                return View(userDto);
            }
            catch (NotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please login to edit your profile.";
                return RedirectToAction("Login");
            }

            try
            {
                var userDto = await _authService.GetUserByIdAsync(userId.Value);

                var model = new UpdateProfileViewModel 
                {
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    Phone = userDto.Phone
                };

                return View(model);
            }
            catch (NotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(UpdateProfileViewModel model) 
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please login to edit your profile.";
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var dto = new UpdateProfileDto
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Phone = model.Phone
                };

                var userDto = await _authService.UpdateProfileAsync(userId.Value, dto);

                HttpContext.Session.SetString("UserName", userDto.FullName);

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(forgotPasswordDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = new ForgotPasswordViewModel
                {
                    Email = dto.Email
                };
                return View(viewModel);
            }

            try
            {
                var token = await _authService.GeneratePasswordResetTokenAsync(dto.Email);

                TempData["SuccessMessage"] = "If your email exists in our system, you will receive a password reset link shortly.";

                return RedirectToAction("ForgotPasswordConfirmation");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred. Please try again.");

                var viewModel = new ForgotPasswordViewModel
                {
                    Email = dto.Email
                };
                return View(viewModel);
            }
        }


        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Invalid reset link.";
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel 
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model) 
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var dto = new resetPasswordDTO
                {
                    Token = model.Token,
                    Email = model.Email,
                    NewPassword = model.NewPassword
                };

                await _authService.ResetPasswordAsync(dto);
                TempData["SuccessMessage"] = "Password reset successfully! Please login with your new password.";
                return RedirectToAction("Login");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please login to change your password.";
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please login to change your password.";
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var dto = new ChangePasswordDTO
                {
                    UserId = userId.Value,
                    CurrentPassword = model.CurrentPassword,
                    NewPassword = model.NewPassword
                };

                await _authService.ChangePasswordAsync(dto);

                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction("Profile");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }
    }
}

