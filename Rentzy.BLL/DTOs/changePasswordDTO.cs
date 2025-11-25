namespace Rentzy.BLL.DTOs
{
    public class ChangePasswordDTO
    {
        public int UserId { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}