namespace Rentzy.Web.Models
{
    public class ReportsVM
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }

        public int TotalProperties { get; set; }
        public int PendingProperties { get; set; }
        public int ApprovedProperties { get; set; }

        public int LandlordPending { get; set; }
        public int LandlordApproved { get; set; }
    }

}
