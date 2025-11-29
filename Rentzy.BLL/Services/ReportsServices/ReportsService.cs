using Rentzy.BLL.DTOs.ReportDTOs;
using Rentzy.DAL.Repository.Reports;

namespace Rentzy.BLL.Services.ReportsServices
{
    public class ReportsService
    {
        private readonly ReportsRepository _repo;

        public ReportsService(ReportsRepository repo)
        {
            _repo = repo;
        }

        public async Task<DashboardReportDto> GetDashboardReportsForLandlordAsync(int landlordId)
        {
            var bookingStatusData = await _repo.GetBookingStatusCountAsync(landlordId);
            var monthlyBookingsData = await _repo.GetMonthlyBookingsAsync(landlordId);
            var monthlyRevenueData = await _repo.GetMonthlyRevenueAsync(landlordId);

            var dto = new DashboardReportDto
            {
                BookingStatusCount = bookingStatusData
                    .Select(x => new BookingStatusCountDto { Status = x.Key, Count = x.Value })
                    .ToList(),

                MonthlyBookings = monthlyBookingsData
                    .Select(x => new MonthlyBookingDto { Year = x.Year, Month = x.Month, Count = x.Count })
                    .ToList(),

                MonthlyRevenue = monthlyRevenueData
                    .Select(x => new MonthlyRevenueDto { Year = x.Year, Month = x.Month, TotalRevenue = x.TotalRevenue })
                    .ToList()
            };

            return dto;
        }
    }
}
