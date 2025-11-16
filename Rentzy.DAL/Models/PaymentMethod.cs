using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.DAL.Models
{
    public class PaymentMethod
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;  // e.g., Credit Card, Bank Transfer, Cash

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

}
