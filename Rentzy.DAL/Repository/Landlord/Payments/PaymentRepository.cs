using Microsoft.EntityFrameworkCore;
using Rentzy.DAL.Context;
using Rentzy.DAL.Models;
using System;

namespace Rentzy.DAL.Repositories
{
    public class PaymentRepository
    {
        private readonly RentzyDBContext _context;

        public PaymentRepository(RentzyDBContext context)
        {
            _context = context;
        }

        public async Task<int> GetPaymentStatusId(string statusName)
        {
            var status = await _context.PaymentStatuses
                .FirstOrDefaultAsync(s => s.Name == statusName);

            if (status == null)
                throw new Exception($"PaymentStatus '{statusName}' not found.");

            return status.Id;
        }


        public async Task<List<Payment>> GetPaymentsByLandlordAsync(int landlordId)
        {
            return await _context.Payments
                .Include(p => p.Booking).ThenInclude(b => b.Property)
                .Include(p => p.Booking).ThenInclude(b => b.Tenant)
                .Include(p => p.Status)
                .Include(p => p.PaymentMethod)
                .Where(p => p.Booking.Property.LandlordId == landlordId)
                .OrderByDescending(p => p.Id)
                .ToListAsync();
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Booking).ThenInclude(b => b.Tenant)
                .Include(p => p.Booking).ThenInclude(b => b.Property)
                .Include(p => p.Status)
                .Include(p => p.PaymentMethod)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }

        public async Task SendReminderAsync(int paymentId, string msg)
        {
            var reminder = new PaymentNotification
            {
                PaymentId = paymentId,
                SentAt = DateTime.Now,
                Message = msg
            };
            _context.PaymentNotifications.Add(reminder);
            await _context.SaveChangesAsync();
        }
    }
}
