using System.Collections.Generic;

namespace Rentzy.DAL.Models
{
    public class Landlord : User
    {
        public override string Role { get; set; } = "Landlord";
        public bool IsVerified { get; set; } = false;
        public ICollection<Property> Properties { get; set; } = new List<Property>();
    }
}
