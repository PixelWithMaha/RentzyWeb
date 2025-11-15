using System.Collections.Generic;

namespace Rentzy.DAL.Models
{
    public class Property
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal RentAmount { get; set; }

        // Foreign key for landlord
        public int LandlordId { get; set; }
        public virtual Landlord Landlord { get; set; }

        // Navigation property for many-to-many with tenants
        public virtual ICollection<TenantProperty> TenantProperties { get; set; } = new List<TenantProperty>();
    }
}
