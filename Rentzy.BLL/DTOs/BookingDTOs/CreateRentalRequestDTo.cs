using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.BLL.DTOs.BookingDTOs
{
    public class CreateRentalRequestDto
    {
        public int PropertyId { get; set; }
        public int TenantId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
