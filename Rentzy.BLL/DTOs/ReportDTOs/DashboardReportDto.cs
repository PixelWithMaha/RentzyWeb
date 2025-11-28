using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.BLL.DTOs.ReportDTOs
{
    public class DashboardReportDto
    {
        public List<BookingStatusCountDto> BookingStatusCount { get; set; } = new();
        public List<MonthlyBookingDto> MonthlyBookings { get; set; } = new();
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
    }

}
