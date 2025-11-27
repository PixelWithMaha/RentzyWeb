using Rentzy.DAL.Models;

namespace Rentzy.DAL
{
    public class TenantWithProperty
    {
        public Rentzy.DAL.Models.User Tenant { get; set; }   // User with role "Tenant"
        public Rentzy.DAL.Models.Property Property { get; set; }
        public Rentzy.DAL.Models.Booking Booking { get; set; }
    }
}
