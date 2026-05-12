namespace Rentzy.Web.Models
{
    public class ManageUserVM
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
