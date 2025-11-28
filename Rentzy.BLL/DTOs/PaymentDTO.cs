using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.BLL.DTOs
{
    /// <summary>
    /// Data sent to the Payment view / flow.
    /// Based on your DAL models: PropertyRentalRequest -> Booking -> Payment
    /// </summary>
    public class PaymentDTO
    {
        // The rental request id (PropertyRentalRequest.Id)
        public int RequestId { get; set; }

        // Property info (you already use PropertyDTO elsewhere; include minimal fields here)
        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;

        // Tenant (who is paying)
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;

        // Amount to be paid (use decimal for money)
        public decimal Amount { get; set; }

        // Optional: suggested/default payment method id (can be 0 if none)
        public int DefaultPaymentMethodId { get; set; }

        // Helpful metadata
        public DateTime RequestedAt { get; set; }
    }
}
