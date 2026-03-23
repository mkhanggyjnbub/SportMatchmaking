using Microsoft.AspNetCore.Mvc;
using Services.Admin;
using SportMatchmaking.Filters;
using SportMatchmaking.Models;

namespace SportMatchmaking.Controllers
{
    [AdminOnly]
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
            int? creatorUserId,
            int page = 1,
            int pageSize = 10)
        {
            var posts = await _adminPostService.GetPostsAsync(keyword, sportId, status, city, district, creatorUserId);
            var sports = await _adminSportService.GetSportsAsync();
            var users = await _adminUserService.GetUsersAsync();

            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var totalItems = posts.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages == 0)
            {
                totalPages = 1;
            }

            if (page > totalPages)
            {
                page = totalPages;
            }

            var pagedPosts = posts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Keyword = keyword;
            ViewBag.SportId = sportId;
            ViewBag.Status = status;
            ViewBag.City = city;
            ViewBag.District = district;
            ViewBag.CreatorUserId = creatorUserId;
            ViewBag.Sports = sports;
            ViewBag.Users = users;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(pagedPosts);
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

        public async Task<IActionResult> Statistics(int? year)
        {
            var availableYears = await _adminPostService.GetAvailablePostYearsAsync();

            int selectedYear = year ?? DateTime.Now.Year;
            if (!availableYears.Any())
            {
                availableYears = new List<int> { selectedYear };
            }
            else if (!availableYears.Contains(selectedYear))
            {
                selectedYear = availableYears.Max();
            }

            var postCountBySport = await _adminPostService.GetPostCountBySportAsync();
            var postCountBySportOpenOrFull = await _adminPostService.GetPostCountBySportOpenOrFullAsync();
            var weeklyPostCount = await _adminPostService.GetWeeklyPostCountByYearAsync(selectedYear);
            var postCountByStatus = await _adminPostService.GetPostCountByStatusAsync();

            var vm = new AdminPostStatisticsVM
            {
                SelectedYear = selectedYear,
                AvailableYears = availableYears,
                SportLabels = postCountBySport.Select(x => x.SportName).ToList(),
                SportCounts = postCountBySport.Select(x => x.PostCount).ToList(),
                OpenOrFullSportLabels = postCountBySportOpenOrFull.Select(x => x.SportName).ToList(),
                OpenOrFullSportCounts = postCountBySportOpenOrFull.Select(x => x.PostCount).ToList(),
                WeekLabels = Enumerable.Range(1, 53).ToList(),
                WeeklyPostCounts = Enumerable.Range(1, 53).Select(week => weeklyPostCount.GetValueOrDefault(week)).ToList(),
                StatusLabels = postCountByStatus
                    .OrderBy(x => x.Key)
                    .Select(x => GetPostStatusText(x.Key))
                    .ToList(),
                StatusCounts = postCountByStatus
                    .OrderBy(x => x.Key)
                    .Select(x => x.Value)
                    .ToList(),
                TotalSports = postCountBySport.Count,
                TotalPosts = postCountBySport.Sum(x => x.PostCount)
            };

            return View(vm);
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

        private static string GetPostStatusText(byte status)
        {
            return status switch
            {
                1 => "Open",
                2 => "Full",
                3 => "Confirmed",
                4 => "Completed",
                5 => "Cancelled",
                6 => "Expired",
                _ => "Unknown"
            };
        }
    }
}