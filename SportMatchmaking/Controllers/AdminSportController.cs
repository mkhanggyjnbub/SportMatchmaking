using BusinessObjects;
using Microsoft.AspNetCore.Mvc;
using Services.Admin;
using SportMatchmaking.Filters;

namespace SportMatchmaking.Controllers
{
    [AdminOnly]
    public class AdminSportController : Controller
    {
        private readonly IAdminSportService _adminSportService;

        public AdminSportController(IAdminSportService adminSportService)
        {
            _adminSportService = adminSportService;
        }

        public async Task<IActionResult> Index(string? keyword)
        {
            var sports = await _adminSportService.GetSportsAsync(keyword);
            ViewBag.Keyword = keyword;

            return View(sports);
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

        public async Task<IActionResult> Images()
        {
            var images = await _adminSportService.GetSportImagesAsync();
            return View(images);
        }

        [HttpGet]
        public IActionResult CreateImage()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateImage(SportImage sportImage)
        {
            var result = await _adminSportService.AddSportImageAsync(sportImage);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(sportImage);
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

            return View(image);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditImage(SportImage sportImage)
        {
            var result = await _adminSportService.UpdateSportImageAsync(sportImage);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(sportImage);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Images));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var result = await _adminSportService.DeleteSportImageAsync(id);

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Images));
        }
    }
}