using BusinessObjects;
using BusinessObjects.Enums;
using Repositories.MatchPosts;
using Services.DTOs;

namespace Services.MatchPosts
{
    public class MatchPostService : IMatchPostService
    {
        private readonly IMatchPostRepository _matchPostRepository;

        private const int MaxSlotsNeeded = 50;
        private const byte MinSkillLevel = 1;
        private const byte MaxSkillLevel = 10;
        private const byte MinReportReasonCode = 1;
        private const byte MaxReportReasonCode = 5;

        public MatchPostService(IMatchPostRepository matchPostRepository)
        {
            _matchPostRepository = matchPostRepository;
        }

        public List<MatchPost> GetPosts(MatchPostSearchDTO? search = null)
        {
            search ??= new MatchPostSearchDTO();

            var posts = _matchPostRepository.GetQueryable().ToList();
            SyncStatuses(posts);

            return ApplyFilters(posts, search)
                .OrderByDescending(x => x.IsUrgent)
                .ThenBy(x => x.StartTime)
                .ThenByDescending(x => x.CreatedAt)
                .ToList();
        }

        public List<MatchPost> GetPostsByCreator(int creatorUserId, MatchPostSearchDTO? search = null)
        {
            search ??= new MatchPostSearchDTO();
            search.CreatorUserId = creatorUserId;
            search.ExploreOnlyActivePosts = false;

            return GetPosts(search);
        }

        public MatchPost? GetById(long postId)
        {
            var post = _matchPostRepository.GetById(postId);
            if (post == null)
            {
                return null;
            }

            SyncStatuses(new[] { post });
            return post;
        }

        public List<Sport> GetSports()
        {
            return _matchPostRepository.GetSports();
        }

        public long Create(CreateMatchPostDTO dto)
        {
            ValidateUser(dto.CreatorUserId);
            ValidatePostPayload(
                dto.SportId,
                dto.Title,
                dto.MatchType,
                dto.StartTime,
                dto.EndTime,
                dto.LocationText,
                dto.GoogleMapsUrl,
                dto.City,
                dto.District,
                dto.SkillMin,
                dto.SkillMax,
                dto.SlotsNeeded,
                dto.FeePerPerson,
                dto.ExpiresAt,
                confirmedParticipantSlots: 0);

            var now = DateTime.Now;
            var post = new MatchPost
            {
                CreatorUserId = dto.CreatorUserId,
                SportId = dto.SportId,
                Title = dto.Title.Trim(),
                MatchType = dto.MatchType,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                LocationText = NormalizeText(dto.LocationText),
                GoogleMapsUrl = NormalizeText(dto.GoogleMapsUrl),
                City = NormalizeText(dto.City),
                District = NormalizeText(dto.District),
                SkillMin = dto.SkillMin,
                SkillMax = dto.SkillMax,
                SlotsNeeded = dto.SlotsNeeded,
                FeePerPerson = dto.FeePerPerson,
                IsUrgent = dto.IsUrgent,
                Description = NormalizeText(dto.Description),
                Status = (byte)PostStatus.Open,
                ExpiresAt = dto.ExpiresAt,
                CreatedAt = now,
                UpdatedAt = null
            };

            var creatorParticipant = new PostParticipant
            {
                Post = post,
                UserId = dto.CreatorUserId,
                Role = PostParticipantRoles.Creator,
                Status = PostParticipantStatuses.Confirmed,
                PartySize = 1,
                JoinedAt = now,
                LeftAt = null
            };

            _matchPostRepository.AddWithCreatorParticipant(post, creatorParticipant);
            return post.PostId;
        }

        public void Update(UpdateMatchPostDTO dto)
        {
            var post = _matchPostRepository.GetById(dto.PostId)
                ?? throw new Exception("Bài đăng không tồn tại.");

            if (post.CreatorUserId != dto.EditorUserId)
            {
                throw new Exception("Bạn không có quyền sửa bài đăng này.");
            }

            if (post.Status == (byte)PostStatus.Completed || post.Status == (byte)PostStatus.Cancelled)
            {
                throw new Exception("Chỉ có thể sửa bài khi bài chưa hoàn thành và chưa bị hủy.");
            }

            var confirmedParticipantSlots = GetConfirmedParticipantSlots(post);

            ValidatePostPayload(
                dto.SportId,
                dto.Title,
                dto.MatchType,
                dto.StartTime,
                dto.EndTime,
                dto.LocationText,
                dto.GoogleMapsUrl,
                dto.City,
                dto.District,
                dto.SkillMin,
                dto.SkillMax,
                dto.SlotsNeeded,
                dto.FeePerPerson,
                dto.ExpiresAt,
                confirmedParticipantSlots);

            post.SportId = dto.SportId;
            post.Title = dto.Title.Trim();
            post.MatchType = dto.MatchType;
            post.StartTime = dto.StartTime;
            post.EndTime = dto.EndTime;
            post.LocationText = NormalizeText(dto.LocationText);
            post.GoogleMapsUrl = NormalizeText(dto.GoogleMapsUrl);
            post.City = NormalizeText(dto.City);
            post.District = NormalizeText(dto.District);
            post.SkillMin = dto.SkillMin;
            post.SkillMax = dto.SkillMax;
            post.SlotsNeeded = dto.SlotsNeeded;
            post.FeePerPerson = dto.FeePerPerson;
            post.IsUrgent = dto.IsUrgent;
            post.Description = NormalizeText(dto.Description);
            post.ExpiresAt = dto.ExpiresAt;
            post.UpdatedAt = DateTime.Now;

            ApplyDerivedStatus(post, post.UpdatedAt.Value);
            _matchPostRepository.Update(post);
        }

        public void Cancel(long postId, int currentUserId)
        {
            var post = _matchPostRepository.GetById(postId)
                ?? throw new Exception("Bài đăng không tồn tại.");

            if (post.CreatorUserId != currentUserId)
            {
                throw new Exception("Bạn không có quyền hủy bài đăng này.");
            }

            if (post.Status == (byte)PostStatus.Completed)
            {
                throw new Exception("Bài đăng đã hoàn thành nên không thể hủy.");
            }

            if (post.Status == (byte)PostStatus.Cancelled)
            {
                throw new Exception("Bài đăng đã bị hủy trước đó.");
            }

            post.Status = (byte)PostStatus.Cancelled;
            post.UpdatedAt = DateTime.Now;
            _matchPostRepository.Update(post);
        }

        public void UpdateStatus(long postId, int currentUserId, byte status)
        {
            if (status != (byte)PostStatus.Confirmed && status != (byte)PostStatus.Completed)
            {
                throw new Exception("Trạng thái cập nhật không hợp lệ.");
            }

            var post = _matchPostRepository.GetById(postId)
                ?? throw new Exception("Bài đăng không tồn tại.");

            if (post.CreatorUserId != currentUserId)
            {
                throw new Exception("Bạn không có quyền cập nhật trạng thái bài đăng này.");
            }

            var now = DateTime.Now;
            ApplyDerivedStatus(post, now);

            if (status == (byte)PostStatus.Confirmed)
            {
                if (post.Status == (byte)PostStatus.Cancelled)
                {
                    throw new Exception("Bài đăng đã bị hủy.");
                }

                if (post.Status == (byte)PostStatus.Completed)
                {
                    throw new Exception("Bài đăng đã hoàn thành.");
                }

                if (post.Status == (byte)PostStatus.Expired)
                {
                    throw new Exception("Bài đăng đã hết hạn nên không thể chốt.");
                }

                post.Status = (byte)PostStatus.Confirmed;
            }
            else
            {
                if (post.Status == (byte)PostStatus.Cancelled)
                {
                    throw new Exception("Bài đăng đã bị hủy.");
                }

                if (post.Status == (byte)PostStatus.Completed)
                {
                    throw new Exception("Bài đăng đã được đánh dấu hoàn thành trước đó.");
                }

                var completedAt = post.EndTime ?? post.StartTime;
                if (completedAt > now)
                {
                    throw new Exception("Chỉ có thể hoàn thành bài khi trận đã bắt đầu hoặc kết thúc.");
                }

                post.Status = (byte)PostStatus.Completed;
            }

            post.UpdatedAt = now;
            _matchPostRepository.Update(post);
        }

        public void LeavePost(long postId, int currentUserId)
        {
            ValidateUser(currentUserId);

            var post = _matchPostRepository.GetById(postId)
                ?? throw new Exception("Bài đăng không tồn tại.");

            var participant = post.PostParticipants
                .FirstOrDefault(x => x.UserId == currentUserId)
                ?? throw new Exception("Bạn chưa tham gia bài đăng này.");

            if (participant.Role == PostParticipantRoles.Creator)
            {
                throw new Exception("Chủ kèo không thể rời khỏi bài đăng của chính mình.");
            }

            if (participant.Status != PostParticipantStatuses.Confirmed)
            {
                throw new Exception("Bạn không còn ở trạng thái đã chốt trong bài đăng này.");
            }

            if (post.Status == (byte)PostStatus.Cancelled || post.Status == (byte)PostStatus.Completed)
            {
                throw new Exception("Không thể rời kèo khi bài đăng đã hoàn thành hoặc đã bị hủy.");
            }

            var now = DateTime.Now;
            if (post.StartTime <= now)
            {
                throw new Exception("Không thể rời kèo sau khi trận đã bắt đầu.");
            }

            participant.Status = PostParticipantStatuses.Left;
            participant.LeftAt = now;

            ApplyAvailabilityStatusAfterParticipantLeave(post, now);
            post.UpdatedAt = now;

            _matchPostRepository.Save();
        }

        public void UpdateParticipantStatus(long postId, int participantUserId, int actorUserId, bool isAdmin, byte status)
        {
            ValidateUser(actorUserId);

            if (status != PostParticipantStatuses.Left && status != PostParticipantStatuses.NoShow)
            {
                throw new Exception("Trạng thái người tham gia không hợp lệ.");
            }

            var post = _matchPostRepository.GetById(postId)
                ?? throw new Exception("Bài đăng không tồn tại.");

            var isCreator = post.CreatorUserId == actorUserId;
            if (!isCreator && !isAdmin)
            {
                throw new Exception("Bạn không có quyền quản lý người tham gia của bài đăng này.");
            }

            var participant = post.PostParticipants
                .FirstOrDefault(x => x.UserId == participantUserId)
                ?? throw new Exception("Không tìm thấy người tham gia trong bài đăng này.");

            if (participant.Role == PostParticipantRoles.Creator)
            {
                throw new Exception("Không thể cập nhật trạng thái của chủ kèo.");
            }

            if (participant.Status != PostParticipantStatuses.Confirmed)
            {
                throw new Exception("Người tham gia này không còn ở trạng thái đã chốt.");
            }

            var now = DateTime.Now;

            if (status == PostParticipantStatuses.Left)
            {
                if (post.Status == (byte)PostStatus.Cancelled || post.Status == (byte)PostStatus.Completed)
                {
                    throw new Exception("Không thể đánh dấu rời kèo khi bài đăng đã hoàn thành hoặc bị hủy.");
                }

                if (post.StartTime <= now)
                {
                    throw new Exception("Chỉ có thể đánh dấu rời kèo trước khi trận bắt đầu.");
                }

                participant.Status = PostParticipantStatuses.Left;
                participant.LeftAt = now;

                ApplyAvailabilityStatusAfterParticipantLeave(post, now);
            }
            else
            {
                if (post.Status == (byte)PostStatus.Cancelled)
                {
                    throw new Exception("Không thể đánh dấu no-show cho bài đăng đã bị hủy.");
                }

                var matchOccurredAt = post.EndTime ?? post.StartTime;
                if (matchOccurredAt > now)
                {
                    throw new Exception("Chỉ có thể đánh dấu no-show sau khi trận đã diễn ra.");
                }

                participant.Status = PostParticipantStatuses.NoShow;
                participant.LeftAt ??= now;

                ApplyDerivedStatus(post, now);
            }

            post.UpdatedAt = now;
            _matchPostRepository.Save();
        }

        public void ReportPost(CreatePostReportDTO dto)
        {
            ValidateUser(dto.ReporterUserId);

            var post = _matchPostRepository.GetById(dto.PostId)
                ?? throw new Exception("Bài đăng không tồn tại.");

            var now = DateTime.Now;
            if (ApplyDerivedStatus(post, now))
            {
                _matchPostRepository.Save();
            }

            if (post.CreatorUserId == dto.ReporterUserId)
            {
                throw new Exception("Bạn không thể report bài đăng của chính mình.");
            }

            if (post.Status != (byte)PostStatus.Expired)
            {
                throw new Exception("Chi co the report bai dang da het han.");
            }

            if (!HasUserParticipatedInPost(post, dto.ReporterUserId))
            {
                throw new Exception("Chi nhung nguoi da tham gia bai dang nay moi co quyen report.");
            }

            if (dto.ReasonCode < MinReportReasonCode || dto.ReasonCode > MaxReportReasonCode)
            {
                throw new Exception("Lý do report không hợp lệ.");
            }

            var existingBlockedReport = post.Reports.FirstOrDefault(x =>
                x.ReporterUserId == dto.ReporterUserId
                && (x.Status == (byte)ReportStatus.Open
                    || x.Status == (byte)ReportStatus.InReview
                    || x.Status == (byte)ReportStatus.Dismissed));
            if (existingBlockedReport != null)
            {
                if (existingBlockedReport.Status == (byte)ReportStatus.Dismissed)
                {
                    throw new Exception("Report cua ban cho bai dang nay da bi dismiss, ban khong the gui lai.");
                }

                throw new Exception("Bạn đã gửi report cho bài đăng này và đang chờ xử lý.");
            }

            var report = new Report
            {
                ReporterUserId = dto.ReporterUserId,
                TargetType = (byte)ReportTargetType.Post,
                TargetPostId = dto.PostId,
                ReasonCode = dto.ReasonCode,
                Details = NormalizeText(dto.Details),
                Status = (byte)ReportStatus.Open,
                CreatedAt = DateTime.Now
            };

            _matchPostRepository.AddReport(report);
        }

        public int GetRemainingSlots(MatchPost post)
        {
            return Math.Max(0, post.SlotsNeeded - GetConfirmedParticipantSlots(post));
        }

        public int GetFilledSlots(MatchPost post)
        {
            return GetConfirmedParticipantSlots(post);
        }
        private void ValidateUser(int userId)
        {
            if (userId <= 0)
            {
                throw new Exception("Người dùng không hợp lệ.");
            }

            var user = _matchPostRepository.GetUserById(userId);
            if (user == null)
            {
                throw new Exception("Người dùng không tồn tại.");
            }

            if (user.IsBanned)
            {
                throw new Exception("Tài khoản của bạn hiện không thể thực hiện thao tác này.");
            }
        }

        private void ValidatePostPayload(
            int sportId,
            string? title,
            byte matchType,
            DateTime startTime,
            DateTime? endTime,
            string? locationText,
            string? googleMapsUrl,
            string? city,
            string? district,
            byte? skillMin,
            byte? skillMax,
            int slotsNeeded,
            decimal? feePerPerson,
            DateTime? expiresAt,
            int confirmedParticipantSlots)
        {
            if (_matchPostRepository.GetSportById(sportId) == null)
            {
                throw new Exception("Môn thể thao không tồn tại.");
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new Exception("Tiêu đề bài đăng là bắt buộc.");
            }

            if (title.Trim().Length > 150)
            {
                throw new Exception("Tiêu đề không được vượt quá 150 ký tự.");
            }

            if (matchType < 1 || matchType > 4)
            {
                throw new Exception("Loại trận không hợp lệ.");
            }

            if (startTime <= DateTime.Now)
            {
                throw new Exception("Thời gian bắt đầu phải ở tương lai.");
            }

            if (!endTime.HasValue)
            {
                throw new Exception("Thời gian kết thúc trận là bắt buộc.");
            }

            if (!expiresAt.HasValue)
            {
                throw new Exception("Hạn chốt là bắt buộc.");
            }

            if (endTime.HasValue && endTime.Value <= startTime)
            {
                throw new Exception("Thời gian kết thúc phải sau thời gian bắt đầu.");
            }

            if (string.IsNullOrWhiteSpace(locationText))
            {
                locationText = "-";
            }

            if (string.IsNullOrWhiteSpace(locationText))
            {
                throw new Exception("Vui lòng nhập địa điểm thi đấu.");
            }

            if (locationText.Trim().Length > 300)
            {
                throw new Exception("Địa điểm không được vượt quá 300 ký tự.");
            }

            Uri? mapsUri = null;
            if (!string.IsNullOrWhiteSpace(googleMapsUrl)
                && !Uri.TryCreate(googleMapsUrl.Trim(), UriKind.Absolute, out mapsUri))
            {
                throw new Exception("Google Maps URL không hợp lệ.");
            }

            if (mapsUri != null
                && mapsUri.Scheme != Uri.UriSchemeHttp
                && mapsUri.Scheme != Uri.UriSchemeHttps)
            {
                throw new Exception("Google Maps URL phải bắt đầu bằng http hoặc https.");
            }

            if (string.IsNullOrWhiteSpace(city))
            {
                city = "-";
            }

            if (string.IsNullOrWhiteSpace(city))
            {
                throw new Exception("Vui lòng nhập thành phố.");
            }

            if (city.Trim().Length > 100)
            {
                throw new Exception("Tên thành phố không được vượt quá 100 ký tự.");
            }

            if (string.IsNullOrWhiteSpace(district))
            {
                district = "-";
            }

            if (string.IsNullOrWhiteSpace(district))
            {
                throw new Exception("Vui lòng nhập quận hoặc huyện.");
            }

            if (district.Trim().Length > 100)
            {
                throw new Exception("Tên quận hoặc huyện không được vượt quá 100 ký tự.");
            }

            ValidateSkillRange(skillMin, skillMax);

            if (slotsNeeded < 1 || slotsNeeded > MaxSlotsNeeded)
            {
                throw new Exception($"Số slot cần thêm phải từ 1 đến {MaxSlotsNeeded}.");
            }

            if (slotsNeeded < confirmedParticipantSlots)
            {
                throw new Exception($"Không thể giảm slot xuống dưới số thành viên đã chốt ({confirmedParticipantSlots}).");
            }

            if (feePerPerson.HasValue && feePerPerson.Value < 0)
            {
                throw new Exception("Chi phí dự kiến không được âm.");
            }

            if (expiresAt.HasValue)
            {
                if (expiresAt.Value <= DateTime.Now)
                {
                    throw new Exception("Hạn chốt phải ở tương lai.");
                }

                if (expiresAt.Value > startTime)
                {
                    throw new Exception("Hạn chốt không được sau thời gian bắt đầu.");
                }
            }
        }

        private static void ValidateSkillRange(byte? skillMin, byte? skillMax)
        {
            if (skillMin.HasValue && (skillMin.Value < MinSkillLevel || skillMin.Value > MaxSkillLevel))
            {
                throw new Exception("Skill tối thiểu phải nằm trong khoảng 1 đến 10.");
            }

            if (skillMax.HasValue && (skillMax.Value < MinSkillLevel || skillMax.Value > MaxSkillLevel))
            {
                throw new Exception("Skill tối đa phải nằm trong khoảng 1 đến 10.");
            }

            if (skillMin.HasValue && skillMax.HasValue && skillMin.Value > skillMax.Value)
            {
                throw new Exception("Skill tối thiểu không được lớn hơn skill tối đa.");
            }
        }

        private IEnumerable<MatchPost> ApplyFilters(IEnumerable<MatchPost> posts, MatchPostSearchDTO search)
        {
            var query = posts;

            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keyword = search.Keyword.Trim();
                query = query.Where(x =>
                    ContainsIgnoreCase(x.Title, keyword) ||
                    ContainsIgnoreCase(x.Description, keyword) ||
                    ContainsIgnoreCase(x.LocationText, keyword) ||
                    ContainsIgnoreCase(x.City, keyword) ||
                    ContainsIgnoreCase(x.District, keyword) ||
                    ContainsIgnoreCase(x.Sport?.Name, keyword));
            }

            if (search.SportId.HasValue)
            {
                query = query.Where(x => x.SportId == search.SportId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.City))
            {
                var city = search.City.Trim();
                query = query.Where(x => ContainsIgnoreCase(x.City, city));
            }

            if (!string.IsNullOrWhiteSpace(search.District))
            {
                var district = search.District.Trim();
                query = query.Where(x => ContainsIgnoreCase(x.District, district));
            }

            if (search.StartFrom.HasValue)
            {
                query = query.Where(x => x.StartTime >= search.StartFrom.Value);
            }

            if (search.StartTo.HasValue)
            {
                query = query.Where(x => x.StartTime <= search.StartTo.Value);
            }

            if (search.SkillLevel.HasValue)
            {
                query = query.Where(x => IsSkillMatch(x, search.SkillLevel.Value));
            }

            if (search.Status.HasValue)
            {
                query = query.Where(x => x.Status == search.Status.Value);
            }
            else if (search.ExploreOnlyActivePosts)
            {
                query = query.Where(x =>
                    x.Status == (byte)PostStatus.Open ||
                    x.Status == (byte)PostStatus.Full ||
                    x.Status == (byte)PostStatus.Confirmed);
            }

            if (search.IsUrgent.HasValue)
            {
                query = query.Where(x => x.IsUrgent == search.IsUrgent.Value);
            }

            if (search.MatchType.HasValue)
            {
                query = query.Where(x => x.MatchType == search.MatchType.Value);
            }

            if (search.CreatorUserId.HasValue)
            {
                query = query.Where(x => x.CreatorUserId == search.CreatorUserId.Value);
            }
            else if (!search.Status.HasValue)
            {
                query = query.Where(x => x.Status != (byte)PostStatus.Cancelled);
            }

            query = query.Where(x => CanViewerSeePost(x, search.ViewerUserId));

            return query;
        }
        private void SyncStatuses(IEnumerable<MatchPost> posts)
        {
            var now = DateTime.Now;
            var hasChanges = false;

            foreach (var post in posts)
            {
                if (ApplyDerivedStatus(post, now))
                {
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                _matchPostRepository.Save();
            }
        }

        private bool ApplyDerivedStatus(MatchPost post, DateTime now)
        {
            if (post.Status == (byte)PostStatus.Cancelled
                || post.Status == (byte)PostStatus.Completed)
            {
                return false;
            }

            if (post.Status == (byte)PostStatus.Confirmed)
            {
                var completedAt = post.EndTime ?? post.StartTime;
                if (completedAt > now)
                {
                    return false;
                }

                post.Status = (byte)PostStatus.Completed;
                post.UpdatedAt = now;
                return true;
            }

            var nextStatus = GetDerivedAvailabilityStatus(post, now);
            if (post.Status == nextStatus)
            {
                return false;
            }

            post.Status = nextStatus;
            post.UpdatedAt = now;
            return true;
        }

        private byte GetDerivedAvailabilityStatus(MatchPost post, DateTime now)
        {
            if ((post.ExpiresAt.HasValue && post.ExpiresAt.Value <= now)
                || post.StartTime <= now)
            {
                return (byte)PostStatus.Expired;
            }

            return GetRemainingSlots(post) <= 0
                ? (byte)PostStatus.Full
                : (byte)PostStatus.Open;
        }

        private void ApplyAvailabilityStatusAfterParticipantLeave(MatchPost post, DateTime now)
        {
            if (post.Status == (byte)PostStatus.Cancelled || post.Status == (byte)PostStatus.Completed)
            {
                return;
            }

            post.Status = GetDerivedAvailabilityStatus(post, now);
            post.UpdatedAt = now;
        }

        private int GetConfirmedParticipantSlots(MatchPost post)
        {
            return post.PostParticipants
                .Where(x => x.Role == PostParticipantRoles.Member && x.Status == PostParticipantStatuses.Confirmed)
                .Sum(x => x.PartySize);
        }

        private static bool ContainsIgnoreCase(string? source, string keyword)
        {
            return !string.IsNullOrWhiteSpace(source)
                && source.Contains(keyword, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSkillMatch(MatchPost post, byte skillLevel)
        {
            var matchesMin = !post.SkillMin.HasValue || post.SkillMin.Value <= skillLevel;
            var matchesMax = !post.SkillMax.HasValue || post.SkillMax.Value >= skillLevel;

            return matchesMin && matchesMax;
        }

        private static bool CanViewerSeePost(MatchPost post, int? viewerUserId)
        {
            var now = DateTime.Now;
            var shouldHideFromOutsiders =
                post.Status == (byte)PostStatus.Cancelled
                || post.Status == (byte)PostStatus.Completed
                || (post.Status == (byte)PostStatus.Confirmed && post.StartTime <= now);

            if (!shouldHideFromOutsiders)
            {
                return true;
            }

            if (!viewerUserId.HasValue)
            {
                return false;
            }

            if (post.CreatorUserId == viewerUserId.Value)
            {
                return true;
            }

            return post.PostParticipants.Any(x =>
                x.UserId == viewerUserId.Value
                && (x.Status == PostParticipantStatuses.Confirmed || x.Status == PostParticipantStatuses.NoShow));
        }

        private static bool HasUserParticipatedInPost(MatchPost post, int userId)
        {
            return post.PostParticipants.Any(x =>
                x.UserId == userId
                && x.Role != PostParticipantRoles.Creator
                && (x.Status == PostParticipantStatuses.Confirmed
                    || x.Status == PostParticipantStatuses.Left
                    || x.Status == PostParticipantStatuses.NoShow));
        }

        private static string? NormalizeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
