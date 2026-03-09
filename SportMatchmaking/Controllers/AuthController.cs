using Microsoft.AspNetCore.Mvc;
using Services.Auth;
using Services.DTOs;
using SportMatchmaking.Models;

namespace SportMatchmaking.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var dto = new RegisterUserDTO
                {
                    Email = model.Email,
                    UserName = model.UserName,
                    Password = model.Password,
                    PhoneNumber = model.PhoneNumber,
                    DisplayName = model.DisplayName,
                    Bio = model.Bio,
                    City = model.City,
                    District = model.District
                };

                _authService.Register(dto);

                TempData["Success"] = "Register successful. Please check your email for OTP.";
                TempData["Email"] = model.Email;

                return RedirectToAction("VerifyOtp");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            var model = new VerifyOtpVM();

            if (TempData["Email"] != null)
            {
                model.Email = TempData["Email"]!.ToString()!;
                TempData.Keep("Email");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyOtp(VerifyOtpVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var dto = new VerifyOtpDTO
                {
                    Email = model.Email,
                    OTP = model.OTP
                };

                _authService.VerifyOtp(dto);

                TempData["Success"] = "Email verified successfully. You can login now.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResendOtp(string email)
        {
            try
            {
                _authService.ResendOtp(email);
                TempData["Success"] = "A new OTP has been sent to your email.";
                TempData["Email"] = email;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                TempData["Email"] = email;
            }

            return RedirectToAction("VerifyOtp");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var dto = new LoginDTO
                {
                    Email = model.Email,
                    Password = model.Password
                };

                var user = _authService.Login(dto);

                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserName", user.UserName);
                HttpContext.Session.SetString("Email", user.Email);
                HttpContext.Session.SetString("RoleId", user.RoleId.ToString());

                TempData["Success"] = "Login successful";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

    }
}
