using System.Collections.Generic;

namespace Rentzy.DAL.Models
{
    public class Tenant : User
    {
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<PropertyRentalRequest> RentalRequests { get; set; } = new List<PropertyRentalRequest>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }

}
