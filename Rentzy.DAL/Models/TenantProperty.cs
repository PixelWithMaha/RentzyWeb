using System;

namespace Rentzy.DAL.Models
{
    public class TenantProperty
    {
        public int TenantId { get; set; }
        public virtual Tenant Tenant { get; set; }

        public int PropertyId { get; set; }
        public virtual Property Property { get; set; }

        public DateTime RentedOn { get; set; } = DateTime.Now;
    }
}
