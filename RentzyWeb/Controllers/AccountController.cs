using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Rentzy.BLL.DTOs;
using Rentzy.BLL.Exceptions;
using Rentzy.BLL.Services;
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

        public AccountController(AuthService authService)
        {
            _authService = authService;
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
                return View(model); // ✅ Return ViewModel
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
                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model); // ✅ Return ViewModel
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

        // ✅ LOGIN - Use ViewModel (Already correct!)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model) // ✅ ViewModel
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

                return userDto.UserType switch
                {
                    "Admin" => RedirectToAction("Index", "Admin"),
                    "Landlord" => RedirectToAction("Dashboard", "Landlord"),
                    "Tenant" => RedirectToAction("Dashboard", "Tenant"),
                    _ => RedirectToAction("Index", "Home")
                };
            }
            catch (AuthenticationException)
            {
                ModelState.AddModelError("", "Invalid email or password. Please try again.");
                return View(model);
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                // Temporary debugging
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(model);
            }
        }

        // ✅ LOGOUT - No parameters needed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        // ✅ PROFILE - No parameters (just display)
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
                return View(userDto); // ✅ Can pass DTO to view for display only
            }
            catch (NotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Login");
            }
        }

        // ✅ EDIT PROFILE - Use ViewModel
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

                var model = new UpdateProfileViewModel // ✅ ViewModel
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
        public async Task<IActionResult> EditProfile(UpdateProfileViewModel model) // ✅ ViewModel
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
                // Convert ViewModel → DTO
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

        // ✅ FORGOT PASSWORD - Use ViewModel
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model) // ✅ ViewModel
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var token = await _authService.GeneratePasswordResetTokenAsync(model.Email);

                var resetLink = Url.Action("ResetPassword", "Account",
                    new { token = token, email = model.Email }, Request.Scheme);

                TempData["ResetLink"] = resetLink;
                TempData["UserEmail"] = model.Email;

                return RedirectToAction("EmailPreview");
            }
            catch
            {
                return RedirectToAction("ForgotPasswordConfirmation");
            }
        }

        [HttpGet]
        public IActionResult EmailPreview()
        {
            if (TempData["ResetLink"] == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // ✅ RESET PASSWORD - Use ViewModel
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
                // Convert ViewModel → DTO
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

        // GET: /Account/ChangePassword
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

        // POST: /Account/ChangePassword
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
                // Convert ViewModel → DTO
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

