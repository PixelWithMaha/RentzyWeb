using Rentzy.BLL.DTOs;
using Rentzy.DAL.Models;
using Rentzy.DAL.Repositories;

namespace Rentzy.BLL.Services
{
    public class PaymentService
    {
        private readonly PaymentRepository _paymentRepo;

        public PaymentService(PaymentRepository paymentRepo)
        {
            _paymentRepo = paymentRepo;
        }

        public async Task<List<PaymentDTO>> GetPaymentsForLandlordAsync(int landlordId)
        {
            var payments = await _paymentRepo.GetPaymentsByLandlordAsync(landlordId);

            return payments.Select(p => new PaymentDTO
            {
                Id = p.Id,
                Amount = p.Amount,
                Method = p.Method,
                PaidAt = p.PaidAt,
                BookingId = p.BookingId,
                StatusId = p.StatusId,
                PaymentMethodId = p.PaymentMethodId,

                StatusName = p.Status.Name,
                PaymentMethodName = p.PaymentMethod.Name,
                TenantName = p.Booking.Tenant.FirstName+ " "+p.Booking.Tenant.LastName,
                PropertyTitle = p.Booking.Property.Title
            }).ToList();
        }

        public async Task<bool> SendReminderAsync(int paymentId)
        {
            try
            {
                await _paymentRepo.SendReminderAsync(paymentId,
                    "Please complete your pending payment for your booking.");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task CreateInitialPaymentAsync(int bookingId, decimal amount)
        {
            var payment = new Payment
            {
                BookingId = bookingId,
                Amount = amount,
                StatusId = await _paymentRepo.GetPaymentStatusId("Pending"),
                PaidAt = null,
                Method = "Not Paid",
                PaymentMethodId = 1
            };

            await _paymentRepo.AddAsync(payment);

            await _paymentRepo.SendReminderAsync(payment.Id, "Please complete your first payment.");
        }


        public async Task<PaymentDTO?> GetPaymentDetailsAsync(int id)
        {
            var p = await _paymentRepo.GetByIdAsync(id);
            if (p == null) return null;

            return new PaymentDTO
            {
                Id = p.Id,
                Amount = p.Amount,
                Method = p.Method,
                PaidAt = p.PaidAt,
                BookingId = p.BookingId,
                StatusId = p.StatusId,
                PaymentMethodId = p.PaymentMethodId,
                StatusName = p.Status.Name,
                PaymentMethodName = p.PaymentMethod.Name,
                TenantName = p.Booking.Tenant.FirstName + " " + p.Booking.Tenant.LastName,
                PropertyTitle = p.Booking.Property.Title // use property title
            };

        }
    }
}
