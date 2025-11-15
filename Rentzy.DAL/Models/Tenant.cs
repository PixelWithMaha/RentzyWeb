using System.Collections.Generic;

namespace Rentzy.DAL.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";

        // Navigation property for many-to-many
        public virtual ICollection<TenantProperty> TenantProperties { get; set; } = new List<TenantProperty>();
    }
}
