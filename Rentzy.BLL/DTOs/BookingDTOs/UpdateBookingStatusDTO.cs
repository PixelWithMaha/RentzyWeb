using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.BLL.DTOs.BookingDTOs
{
    public class UpdateBookingStatusDTO
    {
        public int BookingId { get; set; }
        public int BookingStatusId { get; set; }
    }
}
