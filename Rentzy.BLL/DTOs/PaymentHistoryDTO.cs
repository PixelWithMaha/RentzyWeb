using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.BLL.DTOs
{
    public class PaymentHistoryDTO
    {
        public int PaymentId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string LandlordName { get; set; } = string.Empty;
    }
}
