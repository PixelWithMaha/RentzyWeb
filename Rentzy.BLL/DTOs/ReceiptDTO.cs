using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Rentzy.BLL.DTOs
{
    public class ReceiptDTO
    {
        public int PaymentId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string PropertyTitle { get; set; } = string.Empty;
        public string LandlordName { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public string TenantEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public DateTime BookingStartDate { get; set; }
        public DateTime BookingEndDate { get; set; }
        public int BookingDays { get; set; }
        public string PropertyAddress { get; set; } = string.Empty;
        public string LandlordPhone { get; set; } = string.Empty;
        public string LandlordEmail { get; set; } = string.Empty;
    }
}

