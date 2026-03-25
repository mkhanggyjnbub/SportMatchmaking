using BusinessObjects;
using BusinessObjects.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services;
using Services.DTOs;

using Services;
using Services.MatchPosts;
using SportMatchmaking.Filters;
using SportMatchmaking.Models;

namespace SportMatchmaking.Controllers
{
    public class MatchPostController : Controller
    {
        private readonly IMatchPostService _matchPostService;
        //vinh
        private readonly IChatThreadService _chatThreadService;

        //public MatchPostController(IMatchPostService matchPostService)
        //{
        //    _matchPostService = matchPostService;
        //}

        //vinh
        public MatchPostController(
    IMatchPostService matchPostService,
    IChatThreadService chatThreadService)
        {
            _matchPostService = matchPostService;
            _chatThreadService = chatThreadService;
        }

        [HttpGet]
        public IActionResult Index(MatchPostIndexVM model)
        {
            var search = BuildSearch(model, exploreOnlyActivePosts: false);
            var posts = _matchPostService.GetPosts(search);

            PopulateIndexOptions(model);
            model.Posts = posts.Select(MapListItem).ToList();

            return View(model);
        }

        [HttpGet]
        //public IActionResult Details(long id)
        //{
        //    var post = _matchPostService.GetById(id);
        //    if (post == null)
        //    {
        //        TempData["ErrorMessage"] = "Không tìm thấy bài đăng.";
        //        return RedirectToAction(nameof(Index));
        //    }

        //    return View(MapDetail(post, GetCurrentUserId()));
        //}

        //vinh 
        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var currentUserId = GetCurrentUserId();
            var post = _matchPostService.GetById(id);
            if (post == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài đăng.";
                return RedirectToAction(nameof(Index));
            }

            if (!CanCurrentUserViewPost(post, currentUserId))
            {
                TempData["ErrorMessage"] = "Bài đăng này không hiển thị với tài khoản hiện tại.";
                return RedirectToAction(nameof(Index));
            }

            return View(await MapDetail(post, currentUserId));
        }

        [UserOnly]
        [HttpGet]
        public IActionResult Create()
        {
            var startTime = RoundToNextHour(DateTime.Now.AddHours(2));
            var model = new MatchPostFormVM
            {
                StartTime = startTime,
                ExpiresAt = startTime.AddHours(-1)
            };

            PopulateFormOptions(model);
            return View(model);
        }

        [UserOnly]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MatchPostFormVM model)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để tạo bài đăng.";
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                PopulateFormOptions(model);
                return View(model);
            }

            try
            {
                var postId = _matchPostService.Create(new CreateMatchPostDTO
                {
                    CreatorUserId = userId.Value,
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
                });

                TempData["SuccessMessage"] = "Tạo bài đăng thành công.";
                return RedirectToAction(nameof(Details), new { id = postId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                PopulateFormOptions(model);
                return View(model);
            }
        }

        [UserOnly]
        [HttpGet]
        public IActionResult Edit(long id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập trước.";
                return RedirectToAction("Login", "Auth");
            }

            var post = _matchPostService.GetById(id);
            if (post == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài đăng.";
                return RedirectToAction(nameof(MyPosts));
            }

            if (post.CreatorUserId != userId.Value)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền sửa bài đăng này.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var model = new MatchPostFormVM
            {
                PostId = post.PostId,
                SportId = post.SportId,
                Title = post.Title,
                MatchType = post.MatchType,
                StartTime = post.StartTime,
                EndTime = post.EndTime,
                LocationText = post.LocationText ?? "",
                GoogleMapsUrl = post.GoogleMapsUrl,
                City = post.City ?? "",
                District = post.District ?? "",
                SkillMin = post.SkillMin,
                SkillMax = post.SkillMax,
                SlotsNeeded = post.SlotsNeeded,
                FeePerPerson = post.FeePerPerson,
                IsUrgent = post.IsUrgent,
                Description = post.Description,
                ExpiresAt = post.ExpiresAt,
                IsEdit = true
            };

            PopulateFormOptions(model);
            return View(model);
        }

        [UserOnly]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MatchPostFormVM model)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập trước.";
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                model.IsEdit = true;
                PopulateFormOptions(model);
                return View(model);
            }

            if (!model.PostId.HasValue)
            {
                TempData["ErrorMessage"] = "Bài đăng không hợp lệ.";
                return RedirectToAction(nameof(MyPosts));
            }

            try
            {
                _matchPostService.Update(new UpdateMatchPostDTO
                {
                    PostId = model.PostId.Value,
                    EditorUserId = userId.Value,
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
                });

                TempData["SuccessMessage"] = "Cập nhật bài đăng thành công.";
                return RedirectToAction(nameof(Details), new { id = model.PostId.Value });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.IsEdit = true;
                PopulateFormOptions(model);
                return View(model);
            }
        }

        [UserOnly]
        [HttpGet]
        public IActionResult MyPosts(MatchPostIndexVM model)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập trước.";
                return RedirectToAction("Login", "Auth");
            }

            var search = BuildSearch(model, exploreOnlyActivePosts: false);
            var posts = _matchPostService.GetPostsByCreator(userId.Value, search);

            PopulateIndexOptions(model);
            model.Posts = posts.Select(MapListItem).ToList();

            return View(model);
        }

        [UserOnly]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(long id, string? returnAction = null)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập trước.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                _matchPostService.Cancel(id, userId.Value);
                TempData["SuccessMessage"] = "Đã hủy bài đăng.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectAfterAction(id, returnAction);
        }

        [UserOnly]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(long id, byte status, string? returnAction = null)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập trước.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                _matchPostService.UpdateStatus(id, userId.Value, status);
                TempData["SuccessMessage"] = "Cập nhật trạng thái thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectAfterAction(id, returnAction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Leave(long id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập trước.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                _matchPostService.LeavePost(id, userId.Value);
                TempData["SuccessMessage"] = "Bạn đã rời kèo thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateParticipantStatus(long id, int participantUserId, byte status)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập trước.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                _matchPostService.UpdateParticipantStatus(id, participantUserId, userId.Value, IsCurrentUserAdmin(), status);
                TempData["SuccessMessage"] = "Cập nhật trạng thái người tham gia thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [UserOnly]
        [HttpGet]
        public IActionResult Report(long id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập trước.";
                return RedirectToAction("Login", "Auth");
            }

            var post = _matchPostService.GetById(id);
            if (post == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài đăng.";
                return RedirectToAction(nameof(Index));
            }

            if (!CanUserReportPost(post, userId.Value))
            {
                TempData["ErrorMessage"] = "Ban khong co quyen report bai dang nay.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var model = new CreatePostReportVM
            {
                PostId = post.PostId,
                PostTitle = post.Title
            };

            PopulateReportOptions(model);
            return View(model);
        }

        [UserOnly]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Report(CreatePostReportVM model)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập trước.";
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                PopulateReportOptions(model);
                return View(model);
            }

            try
            {
                _matchPostService.ReportPost(new CreatePostReportDTO
                {
                    PostId = model.PostId,
                    ReporterUserId = userId.Value,
                    ReasonCode = model.ReasonCode,
                    Details = model.Details
                });

                TempData["SuccessMessage"] = "Đã gửi report bài đăng.";
                return RedirectToAction(nameof(Details), new { id = model.PostId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                PopulateReportOptions(model);
                return View(model);
            }
        }

        private MatchPostSearchDTO BuildSearch(MatchPostIndexVM model, bool exploreOnlyActivePosts)
        {
            return new MatchPostSearchDTO
            {
                Keyword = model.Keyword,
                SportId = model.SportId,
                City = model.City,
                District = model.District,
                StartFrom = model.StartFrom,
                StartTo = model.StartTo,
                SkillLevel = model.SkillLevel,
                Status = model.Status,
                IsUrgent = model.IsUrgent,
                MatchType = model.MatchType,
                ViewerUserId = GetCurrentUserId(),
                ExploreOnlyActivePosts = exploreOnlyActivePosts
            };
        }
        private MatchPostListItemVM MapListItem(MatchPost post)
        {
            var now = DateTime.Now;

            return new MatchPostListItemVM
            {
                PostId = post.PostId,
                Title = post.Title,
                SportName = post.Sport?.Name ?? $"Sport #{post.SportId}",
                SportImageUrl = GetSportImageUrl(post.Sport),
                MatchTypeText = MatchPostDisplayHelper.GetMatchTypeText(post.MatchType),
                StartTime = post.StartTime,
                EndTime = post.EndTime,
                LocationText = post.LocationText ?? "",
                City = post.City ?? "",
                District = post.District ?? "",
                SkillText = MatchPostDisplayHelper.GetSkillText(post.SkillMin, post.SkillMax),
                FeePerPerson = post.FeePerPerson,
                IsUrgent = post.IsUrgent,
                Status = post.Status,
                StatusText = MatchPostDisplayHelper.GetStatusText(post.Status),
                SlotsNeeded = post.SlotsNeeded,
                SlotsRemaining = _matchPostService.GetRemainingSlots(post),
                CreatorName = post.CreatorUser?.DisplayName ?? post.CreatorUser?.UserName ?? $"User #{post.CreatorUserId}",
                CanEdit = post.Status != (byte)PostStatus.Completed && post.Status != (byte)PostStatus.Cancelled,
                CanCancel = post.Status != (byte)PostStatus.Completed && post.Status != (byte)PostStatus.Cancelled,
                CanConfirm = post.Status != (byte)PostStatus.Completed
                    && post.Status != (byte)PostStatus.Cancelled
                    && post.Status != (byte)PostStatus.Expired,
                CanComplete = post.Status != (byte)PostStatus.Completed
                    && post.Status != (byte)PostStatus.Cancelled
                    && (post.EndTime ?? post.StartTime) <= now
            };
        }
        //vinh
        private async Task<MatchPostDetailVM> MapDetail(MatchPost post, int? currentUserId)
        // private MatchPostDetailVM MapDetail(MatchPost post, int? currentUserId)
        {
            var now = DateTime.Now;
            var isAdmin = IsCurrentUserAdmin();
            var remainingSlots = _matchPostService.GetRemainingSlots(post);
            var filledSlots = _matchPostService.GetFilledSlots(post);
            var isCreator = currentUserId.HasValue && currentUserId.Value == post.CreatorUserId;
            var canManageParticipants = isCreator || isAdmin;
            var alreadyJoined = currentUserId.HasValue
                && post.PostParticipants.Any(x =>
                    x.UserId == currentUserId.Value
                    && x.Status == PostParticipantStatuses.Confirmed);
            var canLeave = currentUserId.HasValue
                && !isCreator
                && post.PostParticipants.Any(x =>
                    x.UserId == currentUserId.Value
                    && x.Role == PostParticipantRoles.Member
                    && x.Status == PostParticipantStatuses.Confirmed)
                && post.Status != (byte)PostStatus.Completed
                && post.Status != (byte)PostStatus.Cancelled
                && post.StartTime > now;
            var hasPendingJoinRequest = currentUserId.HasValue
                && post.JoinRequests.Any(x =>
                    x.RequesterUserId == currentUserId.Value
                    && x.Status == 1);
            var hasActiveReport = currentUserId.HasValue
                && post.Reports.Any(x =>
                    x.ReporterUserId == currentUserId.Value
                    && (x.Status == (byte)ReportStatus.Open || x.Status == (byte)ReportStatus.InReview));
            var canReport = currentUserId.HasValue
                && CanUserReportPost(post, currentUserId.Value)
                && !hasActiveReport;

            //vinh
            long? chatThreadId = null;

            if (currentUserId.HasValue)
            {
                chatThreadId = await _chatThreadService.GetAccessiblePostThreadIdAsync(post.PostId, currentUserId.Value);
            }

            TempData["ChatDebug"] = $"CurrentUserId={currentUserId}, PostId={post.PostId}, ChatThreadId={chatThreadId}";

            return new MatchPostDetailVM
            {
                PostId = post.PostId,
                Title = post.Title,
                SportName = post.Sport?.Name ?? $"Sport #{post.SportId}",
                SportImageUrl = GetSportImageUrl(post.Sport),
                MatchTypeText = MatchPostDisplayHelper.GetMatchTypeText(post.MatchType),
                StartTime = post.StartTime,
                EndTime = post.EndTime,
                LocationText = post.LocationText ?? "",
                GoogleMapsUrl = post.GoogleMapsUrl,
                City = post.City ?? "",
                District = post.District ?? "",
                SkillText = MatchPostDisplayHelper.GetSkillText(post.SkillMin, post.SkillMax),
                SlotsNeeded = post.SlotsNeeded,
                FilledSlots = filledSlots,
                SlotsRemaining = remainingSlots,
                FeePerPerson = post.FeePerPerson,
                IsUrgent = post.IsUrgent,
                Description = post.Description,
                Status = post.Status,
                StatusText = MatchPostDisplayHelper.GetStatusText(post.Status),
                ExpiresAt = post.ExpiresAt,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                ReportCount = post.Reports.Count,
                CreatorUserId = post.CreatorUserId,
                CreatorDisplayName = post.CreatorUser?.DisplayName ?? post.CreatorUser?.UserName ?? $"User #{post.CreatorUserId}",
                CreatorUserName = post.CreatorUser?.UserName ?? $"user_{post.CreatorUserId}",
                CreatorAvatarUrl = string.IsNullOrWhiteSpace(post.CreatorUser?.AvatarUrl)
                    ? "/images/default-avatar.png"
                    : post.CreatorUser.AvatarUrl,
                CreatorCity = post.CreatorUser?.City,
                CreatorDistrict = post.CreatorUser?.District,
                CreatorSkillLevel = post.CreatorUser?.SkillLevel,
                IsCreator = isCreator,
                AlreadyJoined = alreadyJoined,
                HasPendingJoinRequest = hasPendingJoinRequest,
                CanJoin = currentUserId.HasValue
                    && !isCreator
                    && !alreadyJoined
                    && !hasPendingJoinRequest
                    && post.Status == (byte)PostStatus.Open
                    && remainingSlots > 0
                    && (!post.ExpiresAt.HasValue || post.ExpiresAt.Value > now)
                    && post.StartTime > now,
                CanLeave = canLeave,
                CanEdit = isCreator
                    && post.Status != (byte)PostStatus.Completed
                    && post.Status != (byte)PostStatus.Cancelled,
                CanCancel = isCreator
                    && post.Status != (byte)PostStatus.Completed
                    && post.Status != (byte)PostStatus.Cancelled,
                CanManageRequests = isCreator,
                CanManageParticipants = canManageParticipants,
                //vinh
                CanAccessChatRoom = chatThreadId.HasValue,
                ChatThreadId = chatThreadId,
                CanConfirm = isCreator
                    && post.Status != (byte)PostStatus.Completed
                    && post.Status != (byte)PostStatus.Cancelled
                    && post.Status != (byte)PostStatus.Expired,
                CanComplete = isCreator
                    && post.Status != (byte)PostStatus.Completed
                    && post.Status != (byte)PostStatus.Cancelled
                    && (post.EndTime ?? post.StartTime) <= now,
                CanReport = canReport,
                HasActiveReportByCurrentUser = hasActiveReport,
                Participants = post.PostParticipants
                    .Where(x => x.Status != PostParticipantStatuses.Removed)
                    .OrderBy(x => x.Status == PostParticipantStatuses.Confirmed ? 0 : 1)
                    .ThenBy(x => x.Role)
                    .ThenBy(x => x.JoinedAt)
                    .Select(x => new PostParticipantSummaryVM
                    {
                        UserId = x.UserId,
                        DisplayName = x.User?.DisplayName ?? x.User?.UserName ?? $"User #{x.UserId}",
                        AvatarUrl = string.IsNullOrWhiteSpace(x.User?.AvatarUrl)
                            ? "/images/default-avatar.png"
                            : x.User.AvatarUrl,
                        RoleText = x.Role == PostParticipantRoles.Creator ? "Chủ kèo" : "Thành viên",
                        Status = x.Status,
                        StatusText = MatchPostDisplayHelper.GetParticipantStatusText(x.Status),
                        PartySize = x.PartySize,
                        JoinedAt = x.JoinedAt,
                        LeftAt = x.LeftAt,
                        SkillLevel = x.User?.SkillLevel,
                        City = x.User?.City,
                        District = x.User?.District,
                        CanMarkLeft = canManageParticipants
                            && x.Role != PostParticipantRoles.Creator
                            && x.Status == PostParticipantStatuses.Confirmed
                            && post.Status != (byte)PostStatus.Completed
                            && post.Status != (byte)PostStatus.Cancelled
                            && post.StartTime > now,
                        CanMarkNoShow = canManageParticipants
                            && x.Role != PostParticipantRoles.Creator
                            && x.Status == PostParticipantStatuses.Confirmed
                            && post.Status != (byte)PostStatus.Cancelled
                            && (post.EndTime ?? post.StartTime) <= now
                    })
                    .ToList()
            };
        }

        private void PopulateIndexOptions(MatchPostIndexVM model)
        {
            model.SportOptions = _matchPostService.GetSports()
                .Select(x => new SelectListItem
                {
                    Value = x.SportId.ToString(),
                    Text = x.Name,
                    Selected = model.SportId == x.SportId
                })
                .ToList();

            model.StatusOptions = MatchPostDisplayHelper.FilterStatuses
                .Select(x => new SelectListItem
                {
                    Value = x.Value.ToString(),
                    Text = x.Label,
                    Selected = model.Status == x.Value
                })
                .ToList();

            model.MatchTypeOptions = MatchPostDisplayHelper.MatchTypes
                .Select(x => new SelectListItem
                {
                    Value = x.Value.ToString(),
                    Text = x.Label,
                    Selected = model.MatchType == x.Value
                })
                .ToList();
        }

        private void PopulateFormOptions(MatchPostFormVM model)
        {
            var sports = _matchPostService.GetSports();

            model.SportOptions = sports
                .Select(x => new SelectListItem
                {
                    Value = x.SportId.ToString(),
                    Text = x.Name,
                    Selected = model.SportId == x.SportId
                })
                .ToList();

            model.SportImageMap = sports.ToDictionary(
                x => x.SportId,
                x => string.IsNullOrWhiteSpace(x.Image?.ImageUrl) ? "" : x.Image!.ImageUrl);

            if (model.SportId > 0 && model.SportImageMap.TryGetValue(model.SportId, out var sportImageUrl))
            {
                model.SelectedSportImageUrl = string.IsNullOrWhiteSpace(sportImageUrl) ? null : sportImageUrl;
            }
            else
            {
                model.SelectedSportImageUrl = null;
            }

            model.MatchTypeOptions = MatchPostDisplayHelper.MatchTypes
                .Select(x => new SelectListItem
                {
                    Value = x.Value.ToString(),
                    Text = x.Label,
                    Selected = model.MatchType == x.Value
                })
                .ToList();
        }

        private void PopulateReportOptions(CreatePostReportVM model)
        {
            model.ReasonOptions = MatchPostDisplayHelper.ReportReasons
                .Select(x => new SelectListItem
                {
                    Value = x.Value.ToString(),
                    Text = x.Label,
                    Selected = model.ReasonCode == x.Value
                })
                .ToList();
        }

        private IActionResult RedirectAfterAction(long id, string? returnAction)
        {
            if (string.Equals(returnAction, nameof(MyPosts), StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(MyPosts));
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private static DateTime RoundToNextHour(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0)
                .AddHours(1);
        }

        private static string? GetSportImageUrl(Sport? sport)
        {
            return string.IsNullOrWhiteSpace(sport?.Image?.ImageUrl)
                ? null
                : sport.Image.ImageUrl;
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        private bool IsCurrentUserAdmin()
        {
            return string.Equals(HttpContext.Session.GetString("RoleName"), "Admin", StringComparison.OrdinalIgnoreCase);
        }

        private bool CanCurrentUserViewPost(MatchPost post, int? currentUserId)
        {
            if (IsCurrentUserAdmin())
            {
                return true;
            }

            var now = DateTime.Now;
            var shouldHideFromOutsiders =
                post.Status == (byte)PostStatus.Completed
                || (post.Status == (byte)PostStatus.Confirmed && post.StartTime <= now);

            if (!shouldHideFromOutsiders)
            {
                return true;
            }

            if (!currentUserId.HasValue)
            {
                return false;
            }

            if (post.CreatorUserId == currentUserId.Value)
            {
                return true;
            }

            return post.PostParticipants.Any(x =>
                x.UserId == currentUserId.Value
                && (x.Status == PostParticipantStatuses.Confirmed || x.Status == PostParticipantStatuses.NoShow));
        }

        private static bool CanUserReportPost(MatchPost post, int userId)
        {
            if (post.CreatorUserId == userId)
            {
                return false;
            }

            if (post.Status != (byte)PostStatus.Expired)
            {
                return false;
            }

            return post.PostParticipants.Any(x =>
                x.UserId == userId
                && x.Role != PostParticipantRoles.Creator
                && (x.Status == PostParticipantStatuses.Confirmed
                    || x.Status == PostParticipantStatuses.Left
                    || x.Status == PostParticipantStatuses.NoShow));
        }
    }
}
