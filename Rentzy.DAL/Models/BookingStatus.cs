using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.DAL.Models
{
    public class BookingStatus
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Active, Completed, Cancelled
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
