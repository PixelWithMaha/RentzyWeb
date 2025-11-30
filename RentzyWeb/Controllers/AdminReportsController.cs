using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rentzy.DAL.Context;
using Rentzy.DAL.Models;
using Rentzy.Web.Models;

namespace Rentzy.Web.Controllers
{
    public class AdminReportsController : Controller
    {
        private readonly RentzyDBContext _context;

        public AdminReportsController(RentzyDBContext context)
        {
            _context = context;
        }

        //======================
        // MAIN REPORTS PAGE
        //======================
        public async Task<IActionResult> Index()
        {
            var model = new ReportsVM
            {
                TotalUsers = await _context.Users.CountAsync(),
                ActiveUsers = await _context.UserStatuses.CountAsync(x => x.IsActive),

                TotalProperties = await _context.Properties.CountAsync(),
                PendingProperties = await _context.PropertyApprovalRequests.CountAsync(x => x.StatusId == ApprovalStatusConstants.Pending),
                ApprovedProperties = await _context.PropertyApprovalRequests.CountAsync(x => x.StatusId == ApprovalStatusConstants.Approved),

                LandlordPending = await _context.LandlordApprovals.CountAsync(x => x.ApprovalStatusId == ApprovalStatusConstants.Pending),
                LandlordApproved = await _context.LandlordApprovals.CountAsync(x => x.ApprovalStatusId == ApprovalStatusConstants.Approved)
            };

            return View(model);
        }
    }
}
