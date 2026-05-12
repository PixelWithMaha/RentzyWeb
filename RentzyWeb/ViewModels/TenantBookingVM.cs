using Rentzy.DAL.Models;

namespace RentzyWeb.ViewModels
{
    public class TenantBookingVM
    {
        public PropertyRentalRequest Request { get; set; } = null!;
        public bool IsReviewEligible { get; set; }
        public bool HasExistingReview { get; set; }
        public int? ExistingReviewId { get; set; }
    }
}
