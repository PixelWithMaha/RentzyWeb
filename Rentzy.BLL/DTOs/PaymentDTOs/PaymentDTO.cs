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
        public string TenantName { get; set; }=string.Empty;
        public string PropertyTitle { get; set; } = string.Empty;
    }
}
