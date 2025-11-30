using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.BLL.DTOs
{
    public class PaymentNotificationDTO
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsSeen { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedDate { get; set; }
        public int BookingId { get; set; }
        public string LandlordName { get; set; } = string.Empty;
        public string PropertyImage { get; set; } = string.Empty;
    }
}
