using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.Post;
using Services.PostParticipant;
using Services.Sport;
using SportMatchmaking.Filters;
using SportMatchmaking.Models;

namespace SportMatchmaking.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly ISportService _sportService;
        private readonly IPostParticipantService _postParticipantService;

        public PostController(
            IPostService postService,
            ISportService sportService,
            IPostParticipantService postParticipantService)
        {
            _postService = postService;
            _sportService = sportService;
            _postParticipantService = postParticipantService;
        }

        public async Task<IActionResult> Index(string? keyword, int? sportId)
        {
            var posts = await _postService.GetPostsAsync(keyword, sportId);
            var sports = await _sportService.GetSportsAsync();

            ViewBag.Keyword = keyword;
            ViewBag.SportId = sportId;
            ViewBag.Sports = sports;

            return View(posts);
        }

        public async Task<IActionResult> Details(long id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
            {
                TempData["Error"] = "Không tìm th?y bài ??ng.";
                return RedirectToAction(nameof(Index));
            }

            var participants = await _postParticipantService.GetParticipantsAsync(id);

            var confirmedSlots = participants
                .Where(x => x.Role == (byte)BusinessObjects.ParticipantRole.Participant && x.Status == (byte)BusinessObjects.ParticipantStatus.Confirmed)
                .Sum(x => x.PartySize);

            var remainingSlots = post.SlotsNeeded - confirmedSlots;
            if (remainingSlots < 0) remainingSlots = 0;

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            bool isCurrentUserConfirmedParticipant = currentUserId.HasValue && participants.Any(x =>
                x.UserId == currentUserId.Value &&
                x.Role == (byte)BusinessObjects.ParticipantRole.Participant &&
                x.Status == (byte)BusinessObjects.ParticipantStatus.Confirmed);

            ViewBag.Participants = participants;
            ViewBag.ConfirmedSlots = confirmedSlots;
            ViewBag.RemainingSlots = remainingSlots;
            ViewBag.IsCurrentUserConfirmedParticipant = isCurrentUserConfirmedParticipant;

            return View(post);
        }

        [UserOnly]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Sports = await _sportService.GetSportsAsync();

            return View(new CreatePostVM
            {
                StartTime = DateTime.Now.AddDays(1)
            });
        }

        [UserOnly]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Sports = await _sportService.GetSportsAsync();
                return View(model);
            }

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var dto = new CreatePostDTO
            {
                SportId = model.SportId,
                Title = model.Title,
                MatchType = model.MatchType,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                LocationText = model.LocationText,
                GoogleMapsUrl = model.GoogleMapsUrl,
                City = model.City,
                District = model.District,
                SkillMin = model.SkillMin,
                SkillMax = model.SkillMax,
                SlotsNeeded = model.SlotsNeeded,
                FeePerPerson = model.FeePerPerson,
                IsUrgent = model.IsUrgent,
                Description = model.Description,
                ExpiresAt = model.ExpiresAt
            };

            var result = await _postService.CreatePostAsync(dto, userId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                ViewBag.Sports = await _sportService.GetSportsAsync();
                return View(model);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = result.PostId });
        }
    }
}
