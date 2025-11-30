using Rentzy.DAL.Models;

namespace Rentzy.Web.Models
{
    public class AdminDashboardVM
    {
        public List<User> RecentUsers { get; set; }
        public List<LandlordApproval> PendingLandlords { get; set; }
    }
}
