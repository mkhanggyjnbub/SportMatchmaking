using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.JoinRequest;
using SportMatchmaking.Filters;
using SportMatchmaking.Models;

namespace SportMatchmaking.Controllers
{
    [UserOnly]
    public class JoinRequestController : Controller
    {
        private readonly IJoinRequestService _joinRequestService;

        public JoinRequestController(IJoinRequestService joinRequestService)
        {
            _joinRequestService = joinRequestService;
        }

        [HttpGet]
        public IActionResult Create(long postId)
        {
            TempData.Remove("ErrorMessage");
            TempData.Remove("SuccessMessage");

            var vm = new CreateJoinRequestVM
            {
                PostId = postId,
                PartySize = 1
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateJoinRequestVM model)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập trước khi gửi yêu cầu tham gia.";
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var dto = new CreateJoinRequestDTO
                {
                    PostId = model.PostId,
                    RequesterUserId = userId.Value,
                    SkillLevel = model.SkillLevel!.Value,
                    PartySize = model.PartySize,
                    Message = model.Message,
                    GuestNames = model.GuestNames
                };

                _joinRequestService.Create(dto);

                TempData["SuccessMessage"] = "Gửi yêu cầu tham gia thành công.";
                return RedirectToAction(nameof(MyRequests));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }


        [HttpGet]
        public IActionResult PostRequests(long postId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập trước.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var data = _joinRequestService.GetRequestsOfPost(postId, userId.Value);

                var vm = data.Select(x => new PostJoinRequestItemVM
                {
                    RequestId = x.RequestId,
                    RequesterUserId = x.RequesterUserId,
                    RequesterName = x.RequesterName,
                    RequesterSkillLevel = x.RequesterSkillLevel,
                    PartySize = x.PartySize,
                    Message = x.Message,
                    CreatedAt = x.CreatedAt,
                    Status = x.Status
                }).ToList();

                ViewBag.PostId = postId;
                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult MyRequests()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập trước.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var data = _joinRequestService.GetMyRequests(userId.Value);

                var vm = data.Select(x => new MyJoinRequestItemVM
                {
                    RequestId = x.RequestId,
                    PostId = x.PostId,
                    PostTitle = x.PostTitle,
                    PartySize = x.PartySize,
                    Message = x.Message,
                    CreatedAt = x.CreatedAt,
                    Status = x.Status
                }).ToList();

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(long requestId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập trước.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                _joinRequestService.CancelRequest(requestId, userId.Value);
                TempData["SuccessMessage"] = "Hủy request thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(MyRequests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(long requestId, long postId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập trước.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
               //vinh
                await _joinRequestService.AcceptRequest(requestId, userId.Value);
                //_joinRequestService.AcceptRequest(requestId, userId.Value);
                TempData["SuccessMessage"] = "Duyệt request thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(PostRequests), new { postId = postId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(long requestId, long postId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập trước.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                _joinRequestService.RejectRequest(requestId, userId.Value);
                TempData["SuccessMessage"] = "Từ chối request thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(PostRequests), new { postId = postId });
        }
        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }
    }
}
