using Microsoft.AspNetCore.Mvc;
using Services.Admin;
using SportMatchmaking.Filters;

namespace SportMatchmaking.Controllers
{
    [AdminOnly]
    public class AdminReportController : Controller
    {
        private readonly IAdminReportService _adminReportService;

        public AdminReportController(IAdminReportService adminReportService)
        {
            _adminReportService = adminReportService;
        }

        public async Task<IActionResult> Index(
            byte? status,
            byte? targetType,
            byte? reasonCode,
            int? reporterUserId,
            int page = 1,
            int pageSize = 10)
        {
            var reports = await _adminReportService.GetReportsAsync(status, targetType, reasonCode, reporterUserId);

            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var totalItems = reports.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages == 0)
            {
                totalPages = 1;
            }

            if (page > totalPages)
            {
                page = totalPages;
            }

            var pagedReports = reports
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Status = status;
            ViewBag.TargetType = targetType;
            ViewBag.ReasonCode = reasonCode;
            ViewBag.ReporterUserId = reporterUserId;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(pagedReports);
        }

        public async Task<IActionResult> Details(long id)
        {
            var report = await _adminReportService.GetReportByIdAsync(id);
            if (report == null)
            {
                TempData["Error"] = "Không tìm thấy report.";
                return RedirectToAction(nameof(Index));
            }

            return View(report);
        }

        public async Task<IActionResult> Statistics()
        {
            var stats = await _adminReportService.GetReportReasonStatisticsAsync();
            return View(stats);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkInReview(long id)
        {
            int currentAdminUserId = GetCurrentAdminUserId();

            var result = await _adminReportService.MarkInReviewAsync(id, currentAdminUserId);

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(long id, string? resolution)
        {
            int currentAdminUserId = GetCurrentAdminUserId();

            var result = await _adminReportService.ResolveReportAsync(id, currentAdminUserId, resolution);

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dismiss(long id, string? resolution)
        {
            int currentAdminUserId = GetCurrentAdminUserId();

            var result = await _adminReportService.DismissReportAsync(id, currentAdminUserId, resolution);

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(long id, byte status, string? resolution)
        {
            int currentAdminUserId = GetCurrentAdminUserId();

            var result = await _adminReportService.UpdateReportStatusAsync(id, status, currentAdminUserId, resolution);

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        private int GetCurrentAdminUserId()
        {
            var sessionUserId = HttpContext?.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
            {
                return sessionUserId.Value;
            }

            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == "UserId")?.Value;

            if (int.TryParse(userIdClaim, out int userId) && userId > 0)
            {
                return userId;
            }

            return 0;
        }
    }
}