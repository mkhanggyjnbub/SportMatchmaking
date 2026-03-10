using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Services.Models.JoinRequest;
using System.Security.Claims;

namespace SportMatchmaking.Controllers
{
    public class JoinRequestController : Controller
    {
        private readonly IJoinRequestService _joinRequestService;

        public JoinRequestController(IJoinRequestService joinRequestService)
        {
            _joinRequestService = joinRequestService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        [HttpGet]
        public IActionResult Create(long postId)
        {
            var model = new CreateJoinRequestModel
            {
                PostId = postId,
                PartySize = 1
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateJoinRequestModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
                return View(model);

            var result = _joinRequestService.CreateRequest(userId, model);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(MyRequests));
        }

        [HttpGet]
        public IActionResult RequestsByPost(long postId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var model = _joinRequestService.GetRequestsByPostId(postId, userId);
            ViewBag.PostId = postId;
            return View(model);
        }

        [HttpGet]
        public IActionResult MyRequests()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var model = _joinRequestService.GetMyRequests(userId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(long requestId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var result = _joinRequestService.CancelRequest(requestId, userId);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(MyRequests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Review(ReviewJoinRequestModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var result = _joinRequestService.ReviewRequest(userId, model);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(RequestsByPost), new { postId = model.PostId });
        }
    }
}