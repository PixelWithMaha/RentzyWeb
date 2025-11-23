using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.BLL.DTOs.BookingDTOs
{
    public class RentalRequestDTO
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; }
        public int TenantId { get; set; }

        // Keep string for display, but map from ApprovalStatus.Name
        public string Status { get; set; }
    }
}
