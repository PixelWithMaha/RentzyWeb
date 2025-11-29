using Microsoft.AspNetCore.Mvc;
using Rentzy.BLL.Services.ReportsServices;


public class ReportsController : Controller
{
    private readonly ReportsService _service;

    public ReportsController(ReportsService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Dashboard(int landlordId)
    {
        var data = await _service.GetDashboardReportsForLandlordAsync(landlordId);
        return View(data);
    }
}
