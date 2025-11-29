using Microsoft.EntityFrameworkCore;
using Rentzy.DAL.Context;
using Rentzy.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rentzy.BLL.Services
{
    public class TenantPaymentService
    {
        private readonly RentzyDBContext _db;
        private readonly PaymentNotificationService _notificationService;

        public TenantPaymentService(RentzyDBContext db, PaymentNotificationService notificationService)
        {
            _db = db;
            _notificationService = notificationService;
        }

        // Get a specific payment by ID
        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            return await _db.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Property)
                .Include(p => p.Status)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // Mark payment as paid
        public async Task MarkAsPaid(int paymentId)
        {
            var payment = await _db.Payments.FindAsync(paymentId);
            if (payment != null)
            {
                payment.StatusId = 1; // Paid
                payment.PaidAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        // Get all paid payments for tenant
        public async Task<List<Payment>> GetPaidPaymentsAsync(int tenantId)
        {
            return await _db.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Property)
                .Include(p => p.Status)
                .Where(p => p.Booking.TenantId == tenantId && p.StatusId == 1) // Paid
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();
        }

        // Get all pending payments
        public async Task<List<Payment>> GetPendingPaymentsAsync(int tenantId)
        {
            return await _db.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Property)
                .Include(p => p.Status)
                .Where(p => p.Booking.TenantId == tenantId && p.StatusId != 1) // Not Paid
                .OrderByDescending(p => p.Booking.StartDate)
                .ToListAsync();
        }
        public async Task<List<Payment>> GetPaidPaymentssAsync(int tenantId)
        {
            return await _db.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Property)
                .Where(p => p.Booking.TenantId == tenantId && p.StatusId == 1)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();
        }

        // Create initial payment when booking is approved + send notification
        public async Task CreateInitialPaymentAsync(int bookingId, decimal amount)
        {
            var payment = new Payment
            {
                BookingId = bookingId,
                Amount = amount,
                StatusId = 2, // Unpaid
                PaidAt = null,
                Method = "Not Paid",
                PaymentMethodId = 1
            };

            await _db.Payments.AddAsync(payment);
            await _db.SaveChangesAsync();

            // Send notification
            var notif = new PaymentNotification
            {
                PaymentId = payment.Id,
                Message = "Your booking is approved! Please pay now.",
                SentAt = DateTime.UtcNow,
                IsSeen = false
            };

            await _notificationService.AddAsync(notif);
        }
        // Count of rented properties
        public async Task<int> GetRentedPropertiesCountAsync(int tenantId)
        {
            return await _db.Bookings
                .Where(b => b.TenantId == tenantId)
                .CountAsync();
        }

        // Count of active contracts (not ended)
        public async Task<int> GetActiveContractsCountAsync(int tenantId)
        {
            var today = DateTime.UtcNow.Date;
            return await _db.Bookings
                .Where(b => b.TenantId == tenantId && b.EndDate >= today)
                .CountAsync();
        }

    }
}
