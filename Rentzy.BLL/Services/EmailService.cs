using Rentzy.BLL.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Rentzy.BLL.Services
{
    public class EmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string resetUrl)
        {
            var subject = "Reset Your Rentzy Password";
            var body = GetPasswordResetEmailBody(resetToken, resetUrl);

            return SendEmailInBackground(toEmail, subject, body);
        }

        public Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "Welcome to Rentzy!";
            var body = GetWelcomeEmailBody(userName);

            return SendEmailInBackground(toEmail, subject, body);
        }

        // 🔥 DO NOT BLOCK THE MAIN REQUEST
        private Task SendEmailInBackground(string toEmail, string subject, string body)
        {
            return Task.Run(async () =>
            {
                try
                {
                    using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                    {
                        Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword),
                        EnableSsl = _emailSettings.EnableSsl
                    };

                    using var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);

                    // Background send — your UI won’t wait
                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch
                {
                    // Log error, but DO NOT throw (background)
                }
            });
        }
    


// --- Email body methods remain unchanged ---
private string GetPasswordResetEmailBody(string resetToken, string resetUrl)
        {
            // Define colors for the Grey Button style
            const string primaryColor = "#3B82F6"; // Used for headline/branding
            const string buttonBgColor = "#E9ECEF"; // Light Grey background
            const string buttonTextColor = "#343A40"; // Dark Grey text

            return $@"<!DOCTYPE html>
<html>
<head>
<style>
body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
.container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
.content {{ background-color: white; padding: 30px; border-radius: 10px; }}
/* *** MODIFIED: Grey button background with dark text and a border *** */
.button {{ 
    display: inline-block; 
    padding: 12px 30px; 
    background-color: {buttonBgColor}; 
    color: {buttonTextColor}; 
    text-decoration: none; 
    border-radius: 5px; 
    margin: 20px 0; 
    border: 1px solid {buttonTextColor}; /* Dark border for definition */
    font-weight: bold;
}}
.footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
</style>
</head>
<body>
<div class='container'>
<div class='content'>
<h2 style='color: {primaryColor};'>Reset Your Password</h2>
<p>Hello,</p>
<p>We received a request to reset your password for your Rentzy account.</p>
<p>Click the button below to reset your password:</p>
<a href='{resetUrl}' class='button'>Reset Password</a>
<p><strong>This link will expire in 1 hour.</strong></p>
<p>If you didn't request a password reset, you can safely ignore this email.</p>
<p>Best regards,<br>The Rentzy Team</p>
</div>
<div class='footer'>
<p>&copy; 2024 Rentzy - Rental Management Simplified</p>
</div>
</div>
</body>
</html>";
        }

        private string GetWelcomeEmailBody(string userName)
        {
            return $@"<!DOCTYPE html>
<html>
<head>
<style>
body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
.container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
.content {{ background-color: white; padding: 30px; border-radius: 10px; }}
.footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
</style>
</head>
<body>
<div class='container'>
<div class='content'>
<h2 style='color: #3B82F6;'>Welcome to Rentzy!</h2>
<p>Hello {userName},</p>
<p>Thank you for registering with Rentzy - your all-in-one rental management platform.</p>
<p>You can now:</p>
<ul>
<li>Browse available properties</li>
<li>Manage your bookings</li>
<li>Track payments</li>
<li>And much more!</li>
</ul>
<p>If you have any questions, feel free to reach out to our support team.</p>
<p>Best regards,<br>The Rentzy Team</p>
</div>
<div class='footer'>
<p>&copy; 2024 Rentzy - Rental Management Simplified</p>
</div>
</div>
</body>
</html>";
        }
    }
}