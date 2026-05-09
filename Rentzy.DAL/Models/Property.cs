using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rentzy.DAL.Models
{
    public class Property
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int MonthlyRent { get; set; }

        [ForeignKey(nameof(PropertyType))]
        public int PropertyTypeId { get; set; }
        public PropertyType PropertyType { get; set; }

        [ForeignKey(nameof(Landlord))]
        public int LandlordId { get; set; }
        public Landlord Landlord { get; set; }

        [ForeignKey(nameof(City))]
        public int CityId { get; set; }
        public City City { get; set; }


        public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<PropertyRentalRequest> RentalRequests { get; set; } = new List<PropertyRentalRequest>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<PropertyApprovalRequest> ApprovalRequests { get; set; } = new List<PropertyApprovalRequest>();
    }

}
