using System.Collections.Generic;

namespace Rentzy.DAL.Models
{
    public class Landlord
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";

        // Navigation property: all properties owned by landlord
        public ICollection<Property>? Properties { get; set; }
    }
}
