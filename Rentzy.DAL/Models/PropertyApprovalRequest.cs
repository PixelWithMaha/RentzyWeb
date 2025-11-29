using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.DAL.Models
{
    public class PropertyApprovalRequest
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }

        [ForeignKey(nameof(Admin))]
        public int? AdminId { get; set; }
        public Admin? Admin { get; set; }
        public Property? property { get; set; }
        [ForeignKey(nameof(Status))]
        public int StatusId { get; set; }
        public ApprovalStatus Status { get; set; }

        public string? Comments { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
    }

}
