using BusinessObjects;
using Microsoft.AspNetCore.Mvc;
using Services.Admin;

namespace SportMatchmaking.Controllers
{
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

        [HttpGet]
        public async Task<IActionResult> EditRole(int id)
        {
            var user = await _adminUserService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy user.";
                return RedirectToAction(nameof(Index));
            }

            var roles = await _adminUserService.GetRolesAsync();
            ViewBag.Roles = roles;

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(int id, int roleId)
        {
            int currentAdminUserId = GetCurrentAdminUserId();

            var result = await _adminUserService.UpdateUserRoleAsync(id, roleId, currentAdminUserId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;

                var user = await _adminUserService.GetUserByIdAsync(id);
                var roles = await _adminUserService.GetRolesAsync();

                ViewBag.Roles = roles;
                return View(user);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
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