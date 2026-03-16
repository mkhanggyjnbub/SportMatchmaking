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

        public async Task<IActionResult> Index(string? keyword, int? roleId, bool? isBanned)
        {
            var users = await _adminUserService.GetUsersAsync(keyword, roleId, isBanned);
            var roles = await _adminUserService.GetRolesAsync();

            ViewBag.Keyword = keyword;
            ViewBag.RoleId = roleId;
            ViewBag.IsBanned = isBanned;
            ViewBag.Roles = roles;

            return View(users);
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