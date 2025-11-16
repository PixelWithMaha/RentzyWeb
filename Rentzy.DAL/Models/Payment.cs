using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.DAL.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }

        [ForeignKey(nameof(Booking))]
        public int BookingId { get; set; }
        public Booking Booking { get; set; }

        [ForeignKey(nameof(Status))]
        public int StatusId { get; set; }
        public PaymentStatus Status { get; set; }

        [ForeignKey(nameof(PaymentMethod))]
        public int PaymentMethodId { get; set; }        // FK to PaymentMethod
        public PaymentMethod PaymentMethod { get; set; }
    }

}
