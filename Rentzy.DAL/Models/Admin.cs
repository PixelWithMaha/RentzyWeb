using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.DAL.Models
{
    public class Admin : User
    {
        public override string Role { get; set; } = "Admin";
        public ICollection<PropertyApprovalRequest> ApprovalRequests { get; set; } = new List<PropertyApprovalRequest>();
    }
}
