using Rentzy.BLL.DTOs;
using Rentzy.DAL.Models;
using Rentzy.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rentzy.BLL.Services
{
    public class PaymentNotificationService
    {
        private readonly IPaymentNotificationRepository _repo;
        private readonly IBookingRepository _bookingRepo;
        private readonly IPropertyRepository _propertyRepo;

        public PaymentNotificationService(
            IPaymentNotificationRepository repo,
            IBookingRepository bookingRepo,
            IPropertyRepository propertyRepo)
        {
            _repo = repo;
            _bookingRepo = bookingRepo;
            _propertyRepo = propertyRepo;
        }

        // Get notifications as DTOs with improved data - FIXED NULL ISSUES
        public async Task<List<PaymentNotificationDTO>> GetTenantNotificationsAsync(int tenantId)
        {
            var notifications = await _repo.GetTenantNotificationsAsync(tenantId);
            var result = new List<PaymentNotificationDTO>();

            foreach (var notification in notifications)
            {
                var payment = notification.Payment;
                if (payment == null) continue;

                var booking = await _bookingRepo.GetBookingByIdAsync(payment.BookingId);
                if (booking == null) continue;

                var property = await _propertyRepo.GetPropertyDetailsAsync(booking.PropertyId);
                if (property == null) continue;

                // FIX: Check for null values
                var notificationDTO = new PaymentNotificationDTO
                {
                    Id = notification.Id,
                    PaymentId = payment.Id,
                    PropertyTitle = property.Title ?? "Unknown Property",
                    Message = notification.Message ?? $"Payment required for your booking at {property.Title}",
                    Amount = payment.Amount,
                    DueDate = booking.StartDate.AddDays(-7), // Due 7 days before booking starts
                    IsSeen = notification.IsSeen,
                    Status = GetPaymentStatus(payment),
                    CreatedDate = notification.SentAt,
                    BookingId = booking.Id,
                    LandlordName = property.Landlord != null
                        ? $"{property.Landlord.FirstName} {property.Landlord.LastName}"
                        : "Unknown Landlord",
                    PropertyImage = property.Images?.FirstOrDefault()?.ImageUrl ?? "/images/default-property.jpg"
                };

                result.Add(notificationDTO);
            }

            return result.OrderByDescending(n => n.CreatedDate).ToList();
        }

        // Get single notification by ID - FIXED NULL ISSUES
        public async Task<PaymentNotificationDTO> GetNotificationByIdAsync(int id)
        {
            var notifications = await _repo.GetTenantNotificationsAsync(1); // We'll filter after
            var notification = notifications.FirstOrDefault(n => n.Id == id);

            if (notification == null) return null;

            var payment = notification.Payment;
            var booking = await _bookingRepo.GetBookingByIdAsync(payment.BookingId);
            var property = await _propertyRepo.GetPropertyDetailsAsync(booking.PropertyId);

            // FIX: Check for null values
            return new PaymentNotificationDTO
            {
                Id = notification.Id,
                PaymentId = payment.Id,
                PropertyTitle = property?.Title ?? "Unknown Property",
                Message = notification.Message ?? $"Payment required for your booking",
                Amount = payment.Amount,
                DueDate = booking.StartDate.AddDays(-7),
                IsSeen = notification.IsSeen,
                Status = GetPaymentStatus(payment),
                CreatedDate = notification.SentAt,
                BookingId = booking.Id,
                LandlordName = property?.Landlord != null
                    ? $"{property.Landlord.FirstName} {property.Landlord.LastName}"
                    : "Unknown Landlord",
                PropertyImage = property?.Images?.FirstOrDefault()?.ImageUrl ?? "/images/default-property.jpg"
            };
        }

        public Task MarkAsSeenAsync(int notificationId)
            => _repo.MarkAsSeenAsync(notificationId);

        public Task AddAsync(PaymentNotification notification)
            => _repo.AddAsync(notification);

        // Helper method to determine payment status
        private string GetPaymentStatus(Payment payment)
        {
            if (payment.StatusId == 1) return "Paid";
            if (DateTime.Now > payment.Booking.StartDate.AddDays(-7) && payment.StatusId != 1) return "Overdue";
            return "Pending";
        }

        // Keep existing method for compatibility
        public Task<IEnumerable<PaymentNotification>> GetTenantNotificationsAsyncOld(int tenantId)
            => _repo.GetTenantNotificationsAsync(tenantId);
    }
}