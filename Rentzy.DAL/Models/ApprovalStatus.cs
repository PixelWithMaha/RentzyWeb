using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.DAL.Models
{
    public class ApprovalStatus
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Pending, Approved, Rejected, Cancelled
        public ICollection<PropertyRentalRequest> RentalRequests { get; set; } = new List<PropertyRentalRequest>();
        public ICollection<PropertyApprovalRequest> ApprovalRequests { get; set; } = new List<PropertyApprovalRequest>();
    }

}
