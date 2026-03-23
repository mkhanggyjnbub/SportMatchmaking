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
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _authService.RequestPasswordReset(model.Email);
                TempData["Success"] = "If this email is registered and verified, we have sent a reset code.";
                TempData["Email"] = model.Email.Trim();
                return RedirectToAction("ForgotPasswordVerifyOtp");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ForgotPasswordVerifyOtp()
        {
            var model = new VerifyOtpVM();
            if (TempData["Email"] != null)
            {
                model.Email = TempData["Email"]!.ToString()!;
                TempData.Keep("Email");
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return RedirectToAction("ForgotPassword");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPasswordVerifyOtp(VerifyOtpVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var token = _authService.VerifyForgotPasswordOtp(model.Email, model.OTP);
                TempData["ResetToken"] = token;
                return RedirectToAction("ResetPassword");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResendForgotPasswordOtp(string email)
        {
            try
            {
                _authService.ResendForgotPasswordOtp(email);
                TempData["Success"] = "A new code has been sent to your email.";
                TempData["Email"] = email;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                TempData["Email"] = email;
            }

            return RedirectToAction("ForgotPasswordVerifyOtp");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var token = TempData["ResetToken"]?.ToString();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("ForgotPassword");
            }

            TempData.Keep("ResetToken");
            return View(new ResetPasswordVM { ResetToken = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _authService.CompletePasswordReset(model.ResetToken, model.NewPassword);
                TempData["Success"] = "Your password has been updated. You can sign in now.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

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
                HttpContext.Session.SetString("RoleName", user.Role.Name); // Admin hoặc User

                HttpContext.Session.SetString(
           "AvatarUrl",
           string.IsNullOrEmpty(user.AvatarUrl) ? "/images/default-avatar.png" : user.AvatarUrl
       );

                HttpContext.Session.SetString(
                    "DisplayName",
                    !string.IsNullOrEmpty(user.DisplayName) ? user.DisplayName : user.UserName
                );


                // Xác định URL chuyển hướng theo role
                var redirectUrl = Url.Action("Index", user.Role.Name == "Admin" ? "AdminDashboard" : "Home");

                // Trả lại view Login với thông báo và URL chuyển hướng (hiển thị ngay trên trang login)
                ViewBag.LoginSuccess = "Login successful";
                ViewBag.RedirectUrl = redirectUrl;

                return View("Login", model);
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


        public IActionResult AccessDenied()
        {
            return View();
        }


    }
}
