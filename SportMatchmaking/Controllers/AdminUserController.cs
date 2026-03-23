using BusinessObjects;
using Microsoft.AspNetCore.Mvc;
using Services.Admin;
using SportMatchmaking.Filters;

namespace SportMatchmaking.Controllers
{
    [AdminOnly]
    public class AdminUserController : Controller
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUserController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        public async Task<IActionResult> Index(string? keyword, int? roleId, bool? isBanned, int page = 1, int pageSize = 10)
        {
            var users = await _adminUserService.GetUsersAsync(keyword, roleId, isBanned);
            var roles = await _adminUserService.GetRolesAsync();

            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var totalItems = users.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages == 0)
            {
                totalPages = 1;
            }

            if (page > totalPages)
            {
                page = totalPages;
            }

            var pagedUsers = users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Keyword = keyword;
            ViewBag.RoleId = roleId;
            ViewBag.IsBanned = isBanned;
            ViewBag.Roles = roles;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(pagedUsers);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _adminUserService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy user.";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBan(int id)
        {
            int currentAdminUserId = GetCurrentAdminUserId();

            var result = await _adminUserService.ToggleBanAsync(id, currentAdminUserId);

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        private int GetCurrentAdminUserId()
        {
            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == "UserId")?.Value;

            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            return 0;
        }
    }
}