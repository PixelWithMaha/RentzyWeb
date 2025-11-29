using Rentzy.BLL.DTOs;
using Rentzy.DAL.Models;

public interface ITenantBookingService
{
    Task<PropertyDTO> GetPropertyDetailsAsync(int propertyId);
    Task<List<DateTime>> GetBookedDatesAsync(int propertyId);

    Task CreateRentalRequestAsync(PropertyRentalRequest request);

    Task<PaymentDTO> GetPaymentInfoAsync(int requestId);
    Task<Payment> GetPaymentByIdAsync(int paymentId);

    Task MarkPaymentAsPaidAsync(int paymentId);
}
