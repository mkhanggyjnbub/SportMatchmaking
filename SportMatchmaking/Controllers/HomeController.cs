using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SportMatchmaking.Models;

namespace SportMatchmaking.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var roleName = HttpContext.Session.GetString("RoleName");

            if (roleName == "Admin")
            {
                return RedirectToAction("Index", "AdminDashboard");
            }

            return RedirectToAction("Index", "MatchPost");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
