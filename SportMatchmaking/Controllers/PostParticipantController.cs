using Microsoft.AspNetCore.Mvc;
using Services.Post;
using Services.PostParticipant;
using SportMatchmaking.Filters;

namespace SportMatchmaking.Controllers
{
    [LoginRequired]
    public class PostParticipantController : Controller
    {
        private readonly IPostParticipantService _postParticipantService;
        private readonly IPostService _postService;

        public PostParticipantController(
            IPostParticipantService postParticipantService,
            IPostService postService)
        {
            _postParticipantService = postParticipantService;
            _postService = postService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(long postId)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var roleName = HttpContext.Session.GetString("RoleName") ?? string.Empty;
            bool isAdmin = roleName == "Admin";

            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null)
            {
                TempData["Error"] = "Không tìm thấy bài đăng.";
                return RedirectToAction("Index", "Post");
            }

            if (!isAdmin && post.CreatorUserId != userId)
            {
                TempData["Error"] = "Bạn không có quyền xem danh sách người tham gia của bài này.";
                return RedirectToAction("Details", "Post", new { id = postId });
            }

            var participants = await _postParticipantService.GetParticipantsAsync(postId);
            ViewBag.Post = post;

            return View(participants);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(long postId)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId <= 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var result = await _postParticipantService.LeavePostAsync(postId, userId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;

            return RedirectToAction("Details", "Post", new { id = postId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNoShow(long postId, int userId)
        {
            var actorUserId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var roleName = HttpContext.Session.GetString("RoleName") ?? string.Empty;
            bool isAdmin = roleName == "Admin";

            var result = await _postParticipantService.MarkNoShowAsync(
                postId,
                userId,
                actorUserId,
                isAdmin);

            TempData[result.Success ? "Success" : "Error"] = result.Message;

            return RedirectToAction(nameof(Index), new { postId });
        }
    }
}