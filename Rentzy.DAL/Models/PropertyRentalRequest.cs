using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.DAL.Models
{
    public class PropertyRentalRequest
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Tenant))]
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }

        [ForeignKey(nameof(Property))]
        public int PropertyId { get; set; }
        public Property Property { get; set; }

        [ForeignKey(nameof(Status))]
        public int StatusId { get; set; }
        public ApprovalStatus Status { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.Now;
    }

}
