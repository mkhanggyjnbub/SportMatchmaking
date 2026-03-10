using Microsoft.AspNetCore.Mvc;
using Services.Admin;

namespace SportMatchmaking.Controllers
{
    public class AdminPostController : Controller
    {
        private readonly IAdminPostService _adminPostService;
        private readonly IAdminSportService _adminSportService;
        private readonly IAdminUserService _adminUserService;

        public AdminPostController(
            IAdminPostService adminPostService,
            IAdminSportService adminSportService,
            IAdminUserService adminUserService)
        {
            _adminPostService = adminPostService;
            _adminSportService = adminSportService;
            _adminUserService = adminUserService;
        }

        public async Task<IActionResult> Index(
            string? keyword,
            int? sportId,
            byte? status,
            string? city,
            string? district,
            int? creatorUserId)
        {
            var posts = await _adminPostService.GetPostsAsync(keyword, sportId, status, city, district, creatorUserId);
            var sports = await _adminSportService.GetSportsAsync();
            var users = await _adminUserService.GetUsersAsync();

            ViewBag.Keyword = keyword;
            ViewBag.SportId = sportId;
            ViewBag.Status = status;
            ViewBag.City = city;
            ViewBag.District = district;
            ViewBag.CreatorUserId = creatorUserId;
            ViewBag.Sports = sports;
            ViewBag.Users = users;

            return View(posts);
        }

        public async Task<IActionResult> Details(long id)
        {
            var post = await _adminPostService.GetPostByIdAsync(id);
            if (post == null)
            {
                TempData["Error"] = "Không tìm thấy bài đăng.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ReportCount = await _adminPostService.GetReportCountByPostIdAsync(id);

            return View(post);
        }

        public async Task<IActionResult> HighlyReported(int minReportCount = 1)
        {
            var posts = await _adminPostService.GetHighlyReportedPostsAsync(minReportCount);
            ViewBag.MinReportCount = minReportCount;

            return View(posts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(long id)
        {
            var result = await _adminPostService.CancelPostAsync(id);

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(long id, byte status)
        {
            var result = await _adminPostService.UpdatePostStatusAsync(id, status);

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}