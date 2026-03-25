using Microsoft.AspNetCore.Mvc;
using Services.AppUser;
using Services.DTOs;
using SportMatchmaking.Filters;
using SportMatchmaking.Models;

namespace SportMatchmaking.Controllers
{
    [LoginRequired]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(IProfileService profileService, IWebHostEnvironment environment)
        {
            _profileService = profileService;
            _environment = environment;
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        [HttpGet]
        [HttpGet]
        public IActionResult GetProfilePartial()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = _profileService.GetById(userId.Value);
            if (user == null) return NotFound();

            var vm = new ProfileVM
            {
                UserId = user.UserId,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                DisplayName = user.DisplayName ?? "",
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                PhoneNumber = user.PhoneNumber ?? "",
                City = user.City ?? "",
                District = user.District ?? ""
            };

            return PartialView("_ViewProfilePartial", vm);
        }

        [HttpGet]
        [HttpGet]
        public IActionResult GetEditProfilePartial()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = _profileService.GetById(userId.Value);
            if (user == null) return NotFound();

            var vm = new ProfileVM
            {
                UserId = user.UserId,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                DisplayName = user.DisplayName ?? "",
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                PhoneNumber = user.PhoneNumber ?? "",
                City = user.City ?? "",
                District = user.District ?? ""
            };

            return PartialView("_EditProfilePartial", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(ProfileVM model)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var currentUser = _profileService.GetById(userId.Value);
            if (currentUser == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.Email = currentUser.Email;
                model.UserName = currentUser.UserName;
                return PartialView("_EditProfilePartial", model);
            }

            string? avatarPath = currentUser.AvatarUrl;

            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(model.AvatarFile.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("AvatarFile", "Chỉ chấp nhận file ảnh .jpg, .jpeg, .png, .webp");
                    model.Email = currentUser.Email;
                    model.UserName = currentUser.UserName;
                    return PartialView("_EditProfilePartial", model);
                }

                if (model.AvatarFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("AvatarFile", "Kích thước ảnh tối đa 5MB");
                    model.Email = currentUser.Email;
                    model.UserName = currentUser.UserName;
                    return PartialView("_EditProfilePartial", model);
                }

                var folderPath = Path.Combine(_environment.WebRootPath, "uploads", "avatars");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var savePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    model.AvatarFile.CopyTo(stream);
                }

                avatarPath = $"/uploads/avatars/{fileName}";
            }

            var dto = new UpdateProfileDTO
            {
                UserId = userId.Value,
                DisplayName = model.DisplayName,
                AvatarUrl = avatarPath,
                Bio = model.Bio,
                PhoneNumber = model.PhoneNumber,
                City = model.City,
                District = model.District
            };

            try
            {
                _profileService.UpdateProfile(dto);

                // cập nhật session NGAY bằng dữ liệu mới
                HttpContext.Session.SetString(
                    "AvatarUrl",
                    string.IsNullOrEmpty(avatarPath)
                        ? "/images/default-avatar.png"
                        : avatarPath
                );

                HttpContext.Session.SetString(
                    "DisplayName",
                    !string.IsNullOrEmpty(model.DisplayName)
                        ? model.DisplayName
                        : (currentUser.UserName ?? "Account")
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DisplayName", ex.Message);

                model.Email = currentUser.Email ?? "";
                model.UserName = currentUser.UserName ?? "";

                return PartialView("_EditProfilePartial", model);
            }
        }


        [HttpGet]
        public IActionResult GetChangePasswordPartial()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var vm = new ChangePasswordVM();
            return PartialView("_ChangePasswordPartial", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordVM model)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return PartialView("_ChangePasswordPartial", model);
            }

            try
            {
                _profileService.ChangePassword(userId.Value, model.OldPassword, model.NewPassword);
                return Json(new { success = true, message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("OldPassword", ex.Message);
                return PartialView("_ChangePasswordPartial", model);
            }
        }
    }
}
