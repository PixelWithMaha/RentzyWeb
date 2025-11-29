using Rentzy.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rentzy.DAL.Repository
{
    public interface IPaymentNotificationRepository
    {
        Task<IEnumerable<PaymentNotification>> GetTenantNotificationsAsync(int tenantId);
        Task MarkAsSeenAsync(int notificationId);
        Task AddAsync(PaymentNotification notification);
    }
}
