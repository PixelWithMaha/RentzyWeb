using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.DAL.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int Rating { get; set; } // 1-5
        public string Comment { get; set; } = string.Empty;

        [ForeignKey(nameof(Property))]
        public int PropertyId { get; set; }
        public Property Property { get; set; }

        [ForeignKey(nameof(Tenant))]
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
