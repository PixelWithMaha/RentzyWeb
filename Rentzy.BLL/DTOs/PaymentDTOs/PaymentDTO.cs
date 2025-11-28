namespace Rentzy.BLL.DTOs
{
    public class PaymentDTO
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }

        public int BookingId { get; set; }
        public int StatusId { get; set; }
        public int PaymentMethodId { get; set; }

        public string StatusName { get; set; } = string.Empty;
        public string PaymentMethodName { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public string PropertyTitle { get; set; } = string.Empty;

        //Neww
        public int RequestId { get; set; }

        // Property info (you already use PropertyDTO elsewhere; include minimal fields here)
        public int PropertyId { get; set; }

        // Tenant (who is paying)
        public int TenantId { get; set; }
       
        // Optional: suggested/default payment method id (can be 0 if none)
        public int DefaultPaymentMethodId { get; set; }

        // Helpful metadata
        public DateTime RequestedAt { get; set; }
    }
}
