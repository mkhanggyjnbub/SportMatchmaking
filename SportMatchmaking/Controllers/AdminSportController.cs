using BusinessObjects;
using Microsoft.AspNetCore.Mvc;
using Services.Admin;
using SportMatchmaking.Filters;
using SportMatchmaking.Models;

namespace SportMatchmaking.Controllers
{
    [AdminOnly]
    public class AdminSportController : Controller
    {
        private readonly IAdminSportService _adminSportService;
        private readonly IWebHostEnvironment _environment;
        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
        private const long MaxImageFileSize = 5 * 1024 * 1024;

        public AdminSportController(IAdminSportService adminSportService, IWebHostEnvironment environment)
        {
            _adminSportService = adminSportService;
            _environment = environment;
        }

        public async Task<IActionResult> Index(string? keyword, int page = 1, int pageSize = 10)
        {
            var sports = await _adminSportService.GetSportsAsync(keyword);
            ViewBag.Keyword = keyword;

            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var totalItems = sports.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages == 0)
            {
                totalPages = 1;
            }

            if (page > totalPages)
            {
                page = totalPages;
            }

            var pagedSports = sports
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(pagedSports);
        }

        public async Task<IActionResult> Details(int id)
        {
            var sport = await _adminSportService.GetSportByIdAsync(id);
            if (sport == null)
            {
                TempData["Error"] = "Không tìm thấy môn thể thao.";
                return RedirectToAction(nameof(Index));
            }

            return View(sport);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.SportImages = await _adminSportService.GetSportImagesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sport sport)
        {
            if (sport.TeamMin == null || sport.TeamMax == null || sport.ImageId == null)
            {
                TempData["Error"] = "Team Min, Team Max và Ảnh là bắt buộc.";
                ViewBag.SportImages = await _adminSportService.GetSportImagesAsync();
                return View(sport);
            }

            var result = await _adminSportService.AddSportAsync(sport);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                ViewBag.SportImages = await _adminSportService.GetSportImagesAsync();
                return View(sport);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var sport = await _adminSportService.GetSportByIdAsync(id);
            if (sport == null)
            {
                TempData["Error"] = "Không tìm thấy môn thể thao.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.SportImages = await _adminSportService.GetSportImagesAsync();
            return View(sport);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Sport sport)
        {
            if (sport.TeamMin == null || sport.TeamMax == null)
            {
                TempData["Error"] = "Team Min và Team Max không được để trống.";
                ViewBag.SportImages = await _adminSportService.GetSportImagesAsync();
                return View(sport);
            }

            var result = await _adminSportService.UpdateSportAsync(sport);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                ViewBag.SportImages = await _adminSportService.GetSportImagesAsync();
                return View(sport);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _adminSportService.DeleteSportAsync(id);

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // SPORT IMAGE
        // =========================

        public async Task<IActionResult> Images(int page = 1, int pageSize = 10)
        {
            var images = await _adminSportService.GetSportImagesAsync();

            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var totalItems = images.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages == 0)
            {
                totalPages = 1;
            }

            if (page > totalPages)
            {
                page = totalPages;
            }

            var pagedImages = images
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(pagedImages);
        }

        [HttpGet]
        public IActionResult CreateImage()
        {
            return View(new SportImageUploadVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateImage(SportImageUploadVM model)
        {
            var imageUrl = SaveUploadedSportImage(model.ImageFile, null);
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(imageUrl))
            {
                TempData["Error"] = ModelState.Values.SelectMany(x => x.Errors).FirstOrDefault()?.ErrorMessage ?? "Vui lòng chọn file ảnh hợp lệ.";
                return View(model);
            }

            var sportImage = new SportImage
            {
                ImageUrl = imageUrl
            };

            var result = await _adminSportService.AddSportImageAsync(sportImage);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                DeleteSportImageFileIfExists(imageUrl);
                return View(model);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Images));
        }

        [HttpGet]
        public async Task<IActionResult> EditImage(int id)
        {
            var image = await _adminSportService.GetSportImageByIdAsync(id);
            if (image == null)
            {
                TempData["Error"] = "Không tìm thấy ảnh môn thể thao.";
                return RedirectToAction(nameof(Images));
            }

            return View(new SportImageUploadVM
            {
                ImageId = image.ImageId,
                CurrentImageUrl = image.ImageUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditImage(SportImageUploadVM model)
        {
            var existingImage = await _adminSportService.GetSportImageByIdAsync(model.ImageId);
            if (existingImage == null)
            {
                TempData["Error"] = "Không tìm thấy ảnh môn thể thao.";
                return RedirectToAction(nameof(Images));
            }

            var imageUrl = SaveUploadedSportImage(model.ImageFile, existingImage.ImageUrl);
            if (!ModelState.IsValid)
            {
                model.CurrentImageUrl = existingImage.ImageUrl;
                TempData["Error"] = ModelState.Values.SelectMany(x => x.Errors).FirstOrDefault()?.ErrorMessage ?? "Dữ liệu ảnh không hợp lệ.";
                return View(model);
            }

            var updatedImage = new SportImage
            {
                ImageId = model.ImageId,
                ImageUrl = imageUrl
            };

            var result = await _adminSportService.UpdateSportImageAsync(updatedImage);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                if (!string.Equals(imageUrl, existingImage.ImageUrl, StringComparison.OrdinalIgnoreCase))
                {
                    DeleteSportImageFileIfExists(imageUrl);
                }

                model.CurrentImageUrl = existingImage.ImageUrl;
                return View(model);
            }

            if (!string.Equals(imageUrl, existingImage.ImageUrl, StringComparison.OrdinalIgnoreCase))
            {
                DeleteSportImageFileIfExists(existingImage.ImageUrl);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Images));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var existingImage = await _adminSportService.GetSportImageByIdAsync(id);
            var result = await _adminSportService.DeleteSportImageAsync(id);

            if (result.Success && existingImage != null)
            {
                DeleteSportImageFileIfExists(existingImage.ImageUrl);
            }

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Images));
        }

        private string? SaveUploadedSportImage(IFormFile? imageFile, string? currentImageUrl)
        {
            if (imageFile == null || imageFile.Length <= 0)
            {
                if (string.IsNullOrWhiteSpace(currentImageUrl))
                {
                    ModelState.AddModelError(nameof(SportImageUploadVM.ImageFile), "Vui lòng chọn file ảnh.");
                }

                return currentImageUrl;
            }

            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(SportImageUploadVM.ImageFile), "Chỉ chấp nhận file .jpg, .jpeg, .png, .webp.");
                return currentImageUrl;
            }

            if (imageFile.Length > MaxImageFileSize)
            {
                ModelState.AddModelError(nameof(SportImageUploadVM.ImageFile), "Kích thước ảnh tối đa 5MB.");
                return currentImageUrl;
            }

            var folderPath = Path.Combine(_environment.WebRootPath, "uploads", "sports");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var savePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(savePath, FileMode.Create);
            imageFile.CopyTo(stream);

            return $"/uploads/sports/{fileName}";
        }

        private void DeleteSportImageFileIfExists(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            const string uploadPrefix = "/uploads/sports/";
            if (!imageUrl.StartsWith(uploadPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_environment.WebRootPath, relativePath);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}
