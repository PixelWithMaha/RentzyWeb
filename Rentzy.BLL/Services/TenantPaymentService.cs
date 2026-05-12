using Microsoft.EntityFrameworkCore;
using Rentzy.BLL.DTOs;
using Rentzy.DAL.Context;
using Rentzy.DAL.Models;
using Rentzy.DAL.Repositories;
using Rentzy.DAL.Repository;
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
        private readonly IUserRepository _userRepository;
        private readonly IPropertyRepository _propertyRepository;

        public TenantPaymentService(
            RentzyDBContext db,
            PaymentNotificationService notificationService,
            IUserRepository userRepository,
            IPropertyRepository propertyRepository)
        {
            _db = db;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _propertyRepository = propertyRepository;
        }

        // Get a specific payment by ID
        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            return await _db.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Property)
                        .ThenInclude(p => p.Landlord)
                .Include(p => p.Status)
                .Include(p => p.PaymentMethod)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // Process payment with payment method
        public async Task<bool> ProcessPaymentAsync(int paymentId, string paymentMethod)
        {
            try
            {
                var payment = await _db.Payments.FindAsync(paymentId);
                if (payment == null || payment.StatusId == 2) return false; // Already paid

                payment.StatusId = 2; // Paid
                payment.PaidAt = DateTime.UtcNow;
                payment.Method = GetPaymentMethodDisplayName(paymentMethod);
                payment.PaymentMethodId = GetPaymentMethodId(paymentMethod);

                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("PAYMENT ERROR: " + ex.Message);
                Console.WriteLine("INNER: " + ex.InnerException?.Message);

                var failedPayment = await _db.Payments.FindAsync(paymentId);
                if (failedPayment != null)
                {
                    failedPayment.StatusId = 3;
                    await _db.SaveChangesAsync();
                }
                return false;
            }
        }

        // Get payment history as DTOs
        public async Task<List<PaymentHistoryDTO>> GetPaymentHistoryAsync(int tenantId)
        {
            var paidPayments = await _db.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Property)
                        .ThenInclude(p => p.Landlord)
                .Include(p => p.Status)
                .Include(p => p.PaymentMethod)
                .Where(p => p.Booking.TenantId == tenantId && p.StatusId == 2 && p.PaidAt != null)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();

            var history = new List<PaymentHistoryDTO>();

            foreach (var payment in paidPayments)
            {
                var booking = payment.Booking;
                var property = booking.Property;

                if (property == null || payment.PaidAt == null) continue;

                history.Add(new PaymentHistoryDTO
                {
                    PaymentId = payment.Id,
                    PropertyTitle = property.Title ?? "Unknown Property",
                    Amount = payment.Amount,
                    PaymentDate = payment.PaidAt.Value,
                    PaymentMethod = payment.Method ?? "Credit Card",
                    Status = "Paid",
                    StartDate = booking.StartDate,
                    EndDate = booking.EndDate,
                    TransactionId = $"TXN_{payment.Id:000000}",
                    LandlordName = property.Landlord != null
                        ? $"{property.Landlord.FirstName} {property.Landlord.LastName}"
                        : "Unknown Landlord"
                });
            }

            return history;
        }

        // Get receipt data
        public async Task<ReceiptDTO> GetReceiptAsync(int paymentId)
        {
            var payment = await GetPaymentByIdAsync(paymentId);
            if (payment == null) throw new Exception("Payment not found");
            if (payment.PaidAt == null) throw new Exception("Payment date not found");

            var booking = payment.Booking;
            var property = booking.Property;
            var tenant = await _userRepository.GetUserById(booking.TenantId);

            if (property == null) throw new Exception("Property not found");
            if (tenant == null) throw new Exception("Tenant not found");

            var bookingDays = (booking.EndDate - booking.StartDate).Days;

            return new ReceiptDTO
            {
                PaymentId = payment.Id,
                TransactionId = $"TXN_{payment.Id:000000}",
                PropertyTitle = property.Title ?? "Unknown Property",
                LandlordName = property.Landlord != null
                    ? $"{property.Landlord.FirstName} {property.Landlord.LastName}"
                    : "Unknown Landlord",
                TenantName = $"{tenant.FirstName} {tenant.LastName}",
                TenantEmail = tenant.Email,
                Amount = payment.Amount,
                PaymentMethod = payment.Method ?? "Credit Card",
                PaymentDate = payment.PaidAt.Value,
                BookingStartDate = booking.StartDate,
                BookingEndDate = booking.EndDate,
                BookingDays = bookingDays,
                PropertyAddress = await GetPropertyAddress(property.Id),
                LandlordPhone = property.Landlord?.Phone ?? "N/A",
                LandlordEmail = property.Landlord?.Email ?? "N/A"
            };
        }

        // Get all paid payments for tenant
        public async Task<List<Payment>> GetPaidPaymentsAsync(int tenantId)
        {
            return await _db.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Property)
                .Include(p => p.Status)
                .Where(p => p.Booking.TenantId == tenantId && p.StatusId == 2)
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
                .Where(p => p.Booking.TenantId == tenantId && p.StatusId == 1)
                .OrderByDescending(p => p.Booking.StartDate)
                .ToListAsync();
        }

        // Create initial payment when booking is approved + send notification
        public async Task CreateInitialPaymentAsync(int bookingId, decimal amount)
        {
            var payment = new Payment
            {
                BookingId = bookingId,
                Amount = amount,
                StatusId = 1, // Pending
                PaidAt = null,
                Method = "Pending",
                PaymentMethodId = 1
            };

            await _db.Payments.AddAsync(payment);
            await _db.SaveChangesAsync();

            var notif = new PaymentNotification
            {
                PaymentId = payment.Id,
                Message = "Your booking is approved! Please complete the payment.",
                SentAt = DateTime.UtcNow,
                IsSeen = false
            };

            await _notificationService.AddAsync(notif);
        }

        // FIX: Only count approved bookings (StatusId = 2)
        public async Task<int> GetRentedPropertiesCountAsync(int tenantId)
        {
            return await _db.Bookings
                .Where(b => b.TenantId == tenantId && b.StatusId == 2) // 2 = Approved
                .CountAsync();
        }

        // FIX: Only count approved bookings that haven't ended yet
        public async Task<int> GetActiveContractsCountAsync(int tenantId)
        {
            var today = DateTime.UtcNow.Date;
            return await _db.Bookings
                .Where(b => b.TenantId == tenantId
                         && b.StatusId == 2          // 2 = Approved
                         && b.EndDate.Date >= today)
                .CountAsync();
        }

        public async Task<Payment> GetPaymentByRequestIdAsync(int requestId)
        {
            return await _db.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.Property)
                .ThenInclude(p => p.Landlord)
                .FirstOrDefaultAsync(p => p.BookingId == requestId);
        }

        // Helper methods
        private string GetPaymentMethodDisplayName(string method)
        {
            return method switch
            {
                "credit_card" => "Credit Card",
                "paypal" => "PayPal",
                "bank_transfer" => "Bank Transfer",
                "digital_wallet" => "Digital Wallet",
                _ => "Credit Card"
            };
        }

        private int GetPaymentMethodId(string method)
        {
            return method switch
            {
                "credit_card" => 1,
                "paypal" => 2,
                "bank_transfer" => 3,
                "digital_wallet" => 4,
                _ => 1
            };
        }

        private async Task<string> GetPropertyAddress(int propertyId)
        {
            var property = await _propertyRepository.GetPropertyDetailsAsync(propertyId);
            return property != null ? $"{property.Address}, {property.City}" : "Address not available";
        }

        public async Task MarkAsPaid(int paymentId)
        {
            var payment = await _db.Payments.FindAsync(paymentId);
            if (payment != null)
            {
                payment.StatusId = 2;
                payment.PaidAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, int statusId)
        {
            try
            {
                var payment = await _db.Payments.FindAsync(paymentId);
                if (payment == null) return false;

                payment.StatusId = statusId;
                if (statusId == 2)
                {
                    payment.PaidAt = DateTime.Now;
                }

                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
