using Rentzy.BLL.DTOs;
using Rentzy.BLL.Services;
using Rentzy.DAL.Models;
using Rentzy.DAL.Repository;

public class TenantBookingService : ITenantBookingService
{
    private readonly IPropertyRepository _propertyRepo;
    private readonly IRentalRequestRepository _rentalRepo;
    private readonly TenantPaymentService _paymentService;

    public TenantBookingService(
        IPropertyRepository propertyRepo,
        IRentalRequestRepository rentalRepo,
        TenantPaymentService paymentService)
    {
        _propertyRepo = propertyRepo;
        _rentalRepo = rentalRepo;
        _paymentService = paymentService;
    }

    public async Task<PropertyDTO> GetPropertyDetailsAsync(int propertyId)
    {
        var p = await _propertyRepo.GetPropertyDetailsAsync(propertyId);
        if (p == null) return null;

        return new PropertyDTO
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            Rent = (int)p.MonthlyRent,
            CityId = p.CityId,
            PropertyTypeId = p.PropertyTypeId,
            LandlordId = p.LandlordId,
            LandlordName = p.Landlord?.FirstName + " " + p.Landlord?.LastName,
            Images = p.Images?.ToList() ?? new List<PropertyImage>(),
            TenantNames = p.Bookings?.Select(b => b.Tenant.FirstName + " " + b.Tenant.LastName).ToList()
        };
    }

    public Task<List<DateTime>> GetBookedDatesAsync(int propertyId)
    {
        return _propertyRepo.GetBookedDatesForPropertyAsync(propertyId);
    }

    public async Task CreateRentalRequestAsync(PropertyRentalRequest request)
    {
        await _rentalRepo.AddRequestAsync(request);   // NO RETURN VALUE
    }

    public async Task<PaymentDTO> GetPaymentInfoAsync(int requestId)
    {
        var req = await _propertyRepo.GetRentalRequestAsync(requestId);
        if (req == null) return null;

        return new PaymentDTO
        {
            RequestId = req.Id,
            PropertyId = req.PropertyId,
            PropertyTitle = req.Property?.Title,
            TenantId = req.TenantId,
            TenantName = req.Tenant?.FirstName + " " + req.Tenant?.LastName,
            Amount = (decimal)(req.Property?.MonthlyRent ?? 0),
            RequestedAt = req.RequestedAt,
            DefaultPaymentMethodId = 1
        };
    }

    public Task<Payment> GetPaymentByIdAsync(int paymentId)
    {
        return _paymentService.GetPaymentByIdAsync(paymentId);
    }

    public Task MarkPaymentAsPaidAsync(int paymentId)
    {
        return _paymentService.MarkAsPaid(paymentId);
    }
}
