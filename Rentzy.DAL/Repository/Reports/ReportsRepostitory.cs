using Rentzy.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Rentzy.DAL.Context;

namespace Rentzy.DAL.Repository.Reports
{

    public class ReportsRepository
    {
        private readonly RentzyDBContext _ctx;

        public ReportsRepository(RentzyDBContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<Dictionary<string, int>> GetBookingStatusCountAsync(int landlordId)
        {
            var query = from b in _ctx.Bookings
                        join p in _ctx.Properties
                            on b.PropertyId equals p.Id
                        where p.LandlordId == landlordId
                        group b by b.Status.Name into g
                        select new
                        {
                            Status = g.Key,
                            Count = g.Count()
                        };

            var result = await query.ToListAsync();
            return result.ToDictionary(x => x.Status, x => x.Count);
        }

        public async Task<List<(int Year, int Month, int Count)>> GetMonthlyBookingsAsync(int landlordId)
        {
            var query = from b in _ctx.Bookings
                        join p in _ctx.Properties
                            on b.PropertyId equals p.Id
                        where p.LandlordId == landlordId
                        group b by new { b.StartDate.Year, b.StartDate.Month } into g
                        select new
                        {
                            g.Key.Year,
                            g.Key.Month,
                            Count = g.Count()
                        };

            var result = await query
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            return result.Select(x => (x.Year, x.Month, x.Count)).ToList();
        }

        public async Task<List<(int Year, int Month, decimal TotalRevenue)>> GetMonthlyRevenueAsync(int landlordId)
        {
            var query = from b in _ctx.Bookings
                        join p in _ctx.Properties
                            on b.PropertyId equals p.Id
                        where p.LandlordId == landlordId && b.Payment != null && b.Payment.PaidAt != null
                        group b by new { b.StartDate.Year, b.StartDate.Month } into g
                        select new
                        {
                            g.Key.Year,
                            g.Key.Month,
                            Revenue = g.Sum(b => b.Payment.Amount)
                        };

            var result = await query
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            return result.Select(x => (x.Year, x.Month, x.Revenue)).ToList();
        }
    }
}
