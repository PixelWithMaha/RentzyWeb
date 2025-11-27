using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rentzy.DAL.Models
{
    public class PaymentNotification
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Payment))]
        public int PaymentId { get; set; }
        public Payment Payment { get; set; }

        public DateTime SentAt { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsSeen { get; set; } = false; // NEW: tracks if tenant has seen this notification

    }
}
