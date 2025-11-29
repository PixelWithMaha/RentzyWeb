using Rentzy.DAL.Models;
using Rentzy.DAL.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rentzy.BLL.Services
{
    public class PaymentNotificationService
    {
        private readonly IPaymentNotificationRepository _repo;
        public PaymentNotificationService(IPaymentNotificationRepository repo) => _repo = repo;

        public Task<IEnumerable<PaymentNotification>> GetTenantNotificationsAsync(int tenantId)
            => _repo.GetTenantNotificationsAsync(tenantId);

        public Task MarkAsSeenAsync(int notificationId)
            => _repo.MarkAsSeenAsync(notificationId);

        public Task AddAsync(PaymentNotification notification)
            => _repo.AddAsync(notification);
    }
}
