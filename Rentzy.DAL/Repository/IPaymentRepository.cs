// IPaymentRepository.cs
using Rentzy.DAL.Models;

public interface IPaymentRepository
{
    Task<Payment> AddPaymentAsync(Payment payment);
    Task<Payment> GetPaymentByIdAsync(int paymentId);
    Task<IEnumerable<Payment>> GetPaymentsByBookingIdAsync(int bookingId);
    Task UpdatePaymentAsync(Payment payment);
}
