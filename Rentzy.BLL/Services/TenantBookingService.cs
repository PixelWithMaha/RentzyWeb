using Microsoft.EntityFrameworkCore;
using Rentzy.BLL.DTOs;
using Rentzy.BLL.Services;
using Rentzy.DAL.Context;
using Rentzy.DAL.Models;
using Rentzy.DAL.Repository;
using Rentzy.DAL.Repository.Approvals;

public class TenantBookingService : ITenantBookingService
{
    private readonly IPropertyRepository _propertyRepo;
    private readonly IRentalRequestRepository _rentalRepo;
    private readonly TenantPaymentService _paymentService;
    private readonly IPropertyApprovalRequestsRepo _requestRepo;
    private readonly RentzyDBContext _context;
    public TenantBookingService(
        IPropertyRepository propertyRepo,
        IRentalRequestRepository rentalRepo,
        TenantPaymentService paymentService,
        IPropertyApprovalRequestsRepo requestRepo,
        RentzyDBContext context
        )
    {
        _propertyRepo = propertyRepo;
        _rentalRepo = rentalRepo;
        _paymentService = paymentService;
        _requestRepo = requestRepo;
        _context = context;
    }

    public async Task<PropertyDTO> GetPropertyDetailsAsync(int propertyId)
    {
        var p = await _propertyRepo.GetPropertyDetailsAsync(propertyId);
        if (p == null) return null;

        // Get the latest approval request for this property
        var approvalRequest = await _requestRepo.GetByPropertyIdAsync(p.Id);

        var dto = new PropertyDTO
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

        // Populate approval status into DTO
        dto.StatusId = approvalRequest?.StatusId ?? 0;
        dto.IsApproved = (approvalRequest != null && (approvalRequest.StatusId == ApprovalStatusConstants.Approved || approvalRequest.StatusId == 2));

        return dto;
    }

    public Task<List<DateTime>> GetBookedDatesAsync(int propertyId)
    {
        return _propertyRepo.GetBookedDatesForPropertyAsync(propertyId);
    }

    public async Task CreateRentalRequestAsync(PropertyRentalRequest request)
    {
        await _rentalRepo.AddRequestAsync(request);
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

    // ✅ ADD THIS NEW METHOD
    public async Task<Payment> GetPaymentByRequestIdAsync(int requestId)
    {
        // You need to implement this method in your TenantPaymentService
        // For now, let's create a simple implementation
        var payment = await _paymentService.GetPaymentByRequestIdAsync(requestId);
        return payment;
    }

    public Task MarkPaymentAsPaidAsync(int paymentId)
    {
        return _paymentService.MarkAsPaid(paymentId);
    }

    public async Task<Booking> GetBookingByPaymentIdAsync(int paymentId)
    {
        var payment = await _context.Payments
            .Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        return payment?.Booking;
    }
}