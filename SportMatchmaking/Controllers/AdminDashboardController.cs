using Microsoft.AspNetCore.Mvc;
using Services.Admin;

namespace SportMatchmaking.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public AdminDashboardController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var summary = await _adminDashboardService.GetDashboardSummaryAsync();
            return View(summary);
        }
    }
}