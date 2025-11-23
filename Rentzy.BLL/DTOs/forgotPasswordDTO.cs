using System.ComponentModel.DataAnnotations;

namespace Rentzy.BLL.DTOs
{
    public class forgotPasswordDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;
    }
}