using Microsoft.EntityFrameworkCore;
using Rentzy.DAL.Context;
using Rentzy.DAL.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rentzy.DAL.Repository
{
    public class PaymentNotificationRepository : IPaymentNotificationRepository
    {
        private readonly RentzyDBContext _db;
        public PaymentNotificationRepository(RentzyDBContext db) => _db = db;

        public async Task<IEnumerable<PaymentNotification>> GetTenantNotificationsAsync(int tenantId)
        {
            return await _db.PaymentNotifications
                .Include(n => n.Payment)
                    .ThenInclude(p => p.Booking)
                        .ThenInclude(b => b.Property)
                .Where(n => n.Payment.Booking.TenantId == tenantId)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();
        }

        public async Task MarkAsSeenAsync(int notificationId)
        {
            var n = await _db.PaymentNotifications.FindAsync(notificationId);
            if (n != null)
            {
                n.IsSeen = true;
                await _db.SaveChangesAsync();
            }
        }

        public async Task AddAsync(PaymentNotification notification)
        {
            _db.PaymentNotifications.Add(notification);
            await _db.SaveChangesAsync();
        }
    }
}
