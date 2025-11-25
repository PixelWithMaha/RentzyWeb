using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.BLL.DTOs.BookingDTOs
{
    public class PropertyRentalRequestDto
    {
        public int Id { get; set; }

        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;

        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
    }


}
