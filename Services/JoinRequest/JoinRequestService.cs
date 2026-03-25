using BusinessObjects;
using BusinessObjects.Enums;
using Repositories.JoinRequest;
using Repositories.PostParticipant;
using Services.DTOs;
using Services;
using Services.Notifications;

namespace Services.JoinRequest
{
    public class JoinRequestService : IJoinRequestService
    {
        private readonly IJoinRequestRepository _joinRequestRepository;
        private readonly IPostParticipantRepository _postParticipantRepository;
        private readonly INotificationService _notificationService;
        private readonly IChatThreadService _chatThreadService;

        public JoinRequestService(
            IJoinRequestRepository joinRequestRepository,
            IPostParticipantRepository postParticipantRepository,
            INotificationService notificationService,
            IChatThreadService chatThreadService)
        {
            _joinRequestRepository = joinRequestRepository;
            _postParticipantRepository = postParticipantRepository;
            _notificationService = notificationService;
            _chatThreadService = chatThreadService;
        }

        public byte? GetUserSkillLevel(int userId)
        {
            return _joinRequestRepository.GetUserById(userId)?.SkillLevel;
        }

        public void Create(CreateJoinRequestDTO dto)
        {
            var post = _joinRequestRepository.GetPostById(dto.PostId);
            if (post == null)
            {
                throw new Exception("Bài đăng không tồn tại.");
            }

            var requester = _joinRequestRepository.GetUserById(dto.RequesterUserId);
            if (requester == null)
            {
                throw new Exception("Người dùng không tồn tại.");
            }

            if (post.CreatorUserId == dto.RequesterUserId)
            {
                throw new Exception("Bạn không thể gửi yêu cầu vào bài đăng của chính mình.");
            }

            var existingParticipant = _postParticipantRepository.GetByPostAndUser(dto.PostId, dto.RequesterUserId);
            if (existingParticipant != null && existingParticipant.Status == PostParticipantStatuses.Confirmed)
            {
                throw new Exception("Bạn đã tham gia bài đăng này rồi, không thể gửi request thêm.");
            }

            if (dto.SkillLevel.HasValue)
            {
                if (dto.SkillLevel.Value < 1 || dto.SkillLevel.Value > 10)
                {
                    throw new Exception("Kỹ năng của bạn phải từ 1 đến 10.");
                }

                requester.SkillLevel = dto.SkillLevel.Value;
                requester.UpdatedAt = DateTime.Now;
            }

            var effectiveSkillLevel = dto.SkillLevel ?? requester.SkillLevel;
            var hasSkillRequirement = post.SkillMin.HasValue || post.SkillMax.HasValue;

            if (hasSkillRequirement && !effectiveSkillLevel.HasValue)
            {
                throw new Exception("Vui lòng nhập kỹ năng của bạn để gửi request vào kèo này.");
            }

            if (effectiveSkillLevel.HasValue)
            {
                if (post.SkillMin.HasValue && effectiveSkillLevel.Value < post.SkillMin.Value)
                {
                    throw new Exception($"Trình độ của bạn chưa đạt tối thiểu ({post.SkillMin}).");
                }

                if (post.SkillMax.HasValue && effectiveSkillLevel.Value > post.SkillMax.Value)
                {
                    throw new Exception($"Trình độ của bạn vượt mức tối đa ({post.SkillMax}).");
                }
            }

            if (post.ExpiresAt.HasValue && post.ExpiresAt.Value < DateTime.Now)
            {
                throw new Exception("Bài đăng đã hết hạn.");
            }

            if (post.StartTime <= DateTime.Now)
            {
                throw new Exception("Kèo này đã tới giờ bắt đầu hoặc đã diễn ra.");
            }

            if (dto.PartySize < 1 || dto.PartySize > 30)
            {
                throw new Exception("PartySize không hợp lệ.");
            }

            if (post.Status != (byte)PostStatus.Open)
            {
                throw new Exception("Bài đăng hiện tại không ở trạng thái mở.");
            }

            var confirmedParticipantSlots = _joinRequestRepository.GetConfirmedParticipantSlots(dto.PostId);
            var remainingSlots = post.SlotsNeeded - confirmedParticipantSlots;

            if (remainingSlots <= 0)
            {
                throw new Exception("Bài đăng đã đủ người.");
            }

            if (dto.PartySize > remainingSlots)
            {
                throw new Exception($"Số chỗ còn lại không đủ. Hiện chỉ còn {remainingSlots} chỗ.");
            }

            var existingRequest = _joinRequestRepository.GetByPostAndRequester(dto.PostId, dto.RequesterUserId);

            if (existingRequest != null)
            {
                if (existingRequest.Status == 1)
                {
                    throw new Exception("Bạn đã có một yêu cầu đang chờ duyệt ở bài này.");
                }

                if (existingRequest.Status == 2)
                {
                    if (existingParticipant == null || existingParticipant.Status == PostParticipantStatuses.Confirmed)
                    {
                        throw new Exception("Bạn đã được chấp nhận ở bài đăng này rồi.");
                    }

                    existingRequest.PartySize = dto.PartySize;
                    existingRequest.Message = string.IsNullOrWhiteSpace(dto.Message) ? null : dto.Message.Trim();
                    existingRequest.GuestNames = string.IsNullOrWhiteSpace(dto.GuestNames) ? null : dto.GuestNames.Trim();
                    existingRequest.Status = 1;
                    existingRequest.CreatedAt = DateTime.Now;
                    existingRequest.DecidedAt = null;
                    existingRequest.DecidedByUserId = null;

                    _joinRequestRepository.Update(existingRequest);
                    _joinRequestRepository.Save();

                    _notificationService.NotifyNewJoinRequest(existingRequest);
                    return;
                }

                if (existingRequest.Status == 3 || existingRequest.Status == 4)
                {
                    existingRequest.PartySize = dto.PartySize;
                    existingRequest.Message = string.IsNullOrWhiteSpace(dto.Message) ? null : dto.Message.Trim();
                    existingRequest.GuestNames = string.IsNullOrWhiteSpace(dto.GuestNames) ? null : dto.GuestNames.Trim();
                    existingRequest.Status = 1;
                    existingRequest.CreatedAt = DateTime.Now;
                    existingRequest.DecidedAt = null;
                    existingRequest.DecidedByUserId = null;

                    _joinRequestRepository.Update(existingRequest);
                    _joinRequestRepository.Save();

                    _notificationService.NotifyNewJoinRequest(existingRequest);
                    return;
                }

                throw new Exception("Không thể gửi lại request này.");
            }

            var entity = new BusinessObjects.JoinRequest
            {
                PostId = dto.PostId,
                RequesterUserId = dto.RequesterUserId,
                PartySize = dto.PartySize,
                Message = string.IsNullOrWhiteSpace(dto.Message) ? null : dto.Message.Trim(),
                GuestNames = string.IsNullOrWhiteSpace(dto.GuestNames) ? null : dto.GuestNames.Trim(),
                Status = 1,
                CreatedAt = DateTime.Now,
                DecidedAt = null,
                DecidedByUserId = null
            };

            _joinRequestRepository.Add(entity);
            _joinRequestRepository.Save();

            _notificationService.NotifyNewJoinRequest(entity);
        }

        public List<PostJoinRequestItemDTO> GetRequestsOfPost(long postId, int currentUserId)
        {
            var post = _joinRequestRepository.GetPostById(postId);
            if (post == null)
            {
                throw new Exception("Bài đăng không tồn tại.");
            }

            if (post.CreatorUserId != currentUserId)
            {
                throw new Exception("Bạn không có quyền xem danh sách request của bài đăng này.");
            }

            var requests = _joinRequestRepository.GetRequestsByPostId(postId);

            return requests.Select(x => new PostJoinRequestItemDTO
            {
                RequestId = x.RequestId,
                PostId = x.PostId,
                RequesterUserId = x.RequesterUserId,
                RequesterName = !string.IsNullOrWhiteSpace(x.RequesterUser?.DisplayName)
                    ? x.RequesterUser.DisplayName
                    : (x.RequesterUser?.UserName ?? "Unknown"),
                RequesterSkillLevel = x.RequesterUser?.SkillLevel,
                PartySize = x.PartySize,
                Message = x.Message,
                CreatedAt = x.CreatedAt,
                Status = x.Status
            }).ToList();
        }

        public List<MyJoinRequestItemDTO> GetMyRequests(int currentUserId)
        {
            var requests = _joinRequestRepository.GetRequestsByRequesterUserId(currentUserId);

            return requests.Select(x => new MyJoinRequestItemDTO
            {
                RequestId = x.RequestId,
                PostId = x.PostId,
                PostTitle = x.Post != null ? x.Post.Title : "(Khong co tieu de)",
                PartySize = x.PartySize,
                Message = x.Message,
                CreatedAt = x.CreatedAt,
                Status = x.Status
            }).ToList();
        }

        public void CancelRequest(long requestId, int currentUserId)
        {
            var request = _joinRequestRepository.GetById(requestId);
            if (request == null)
            {
                throw new Exception("Request khong ton tai.");
            }

            if (request.RequesterUserId != currentUserId)
            {
                throw new Exception("Ban khong co quyen huy request nay.");
            }

            if (request.Status != 1)
            {
                throw new Exception("Chi co the huy request dang o trang thai Pending.");
            }

            request.Status = 4;

            _joinRequestRepository.Update(request);
            _joinRequestRepository.Save();
        }

        public async Task AcceptRequest(long requestId, int currentUserId)
        {
            var request = _joinRequestRepository.GetById(requestId);
            if (request == null)
            {
                throw new Exception("Request khong ton tai.");
            }

            var post = request.Post ?? _joinRequestRepository.GetPostById(request.PostId);
            if (post == null)
            {
                throw new Exception("Bai dang khong ton tai.");
            }

            if (post.CreatorUserId != currentUserId)
            {
                throw new Exception("Ban khong co quyen duyet request cua bai dang nay.");
            }

            if (request.Status != 1)
            {
                throw new Exception("Request đã được xử lí trước đó");
            }

            var confirmedParticipantSlots = _joinRequestRepository.GetConfirmedParticipantSlots(post.PostId);
            var remainingSlots = post.SlotsNeeded - confirmedParticipantSlots;

            if (remainingSlots <= 0)
            {
                throw new Exception("Bài đăng đã đủ người.");
            }

            if (post.Status != (byte)PostStatus.Open)
            {
                throw new Exception("Bài đăng không còn mở.");
            }

            if (post.ExpiresAt.HasValue && post.ExpiresAt.Value < DateTime.Now)
            {
                throw new Exception("Bài đăng hết hạn.");
            }

           

           

            if (request.PartySize > remainingSlots)
            {
                throw new Exception($"Khong du slot de duyet request nay. Chi con {remainingSlots} cho.");
            }

            request.Status = 2;
            request.DecidedAt = DateTime.Now;
            request.DecidedByUserId = currentUserId;

            var participant = _postParticipantRepository.GetByPostAndUser(post.PostId, request.RequesterUserId);

            if (participant == null)
            {
                participant = new BusinessObjects.PostParticipant
                {
                    PostId = post.PostId,
                    UserId = request.RequesterUserId,
                    Role = PostParticipantRoles.Member,
                    Status = PostParticipantStatuses.Confirmed,
                    PartySize = request.PartySize,
                    JoinedAt = DateTime.Now,
                    LeftAt = null
                };

                _postParticipantRepository.Add(participant);
            }
            else
            {
                participant.Role = PostParticipantRoles.Member;
                participant.Status = PostParticipantStatuses.Confirmed;
                participant.PartySize = request.PartySize;
                participant.LeftAt = null;

                if (participant.JoinedAt == default)
                {
                    participant.JoinedAt = DateTime.Now;
                }

                _postParticipantRepository.Update(participant);
            }

            var updatedConfirmedParticipantSlots = confirmedParticipantSlots + request.PartySize;

            if (updatedConfirmedParticipantSlots >= post.SlotsNeeded)
            {
                post.Status = (byte)PostStatus.Full;
                post.UpdatedAt = DateTime.Now;
                _joinRequestRepository.UpdatePost(post);
            }
            else
            {
                post.Status = (byte)PostStatus.Open;
                post.UpdatedAt = DateTime.Now;
                _joinRequestRepository.UpdatePost(post);
            }

            _joinRequestRepository.Update(request);
            _joinRequestRepository.Save();

            //vinh
            await _chatThreadService.EnsurePostGroupThreadCreatedAsync(post.PostId);
            await _chatThreadService.AddConfirmedParticipantsToThreadAsync(post.PostId);

            _notificationService.NotifyRequestAccepted(request);
        }

        public void RejectRequest(long requestId, int currentUserId)
        {
            var request = _joinRequestRepository.GetById(requestId);
            if (request == null)
            {
                throw new Exception("Request khong ton tai.");
            }

            var post = request.Post ?? _joinRequestRepository.GetPostById(request.PostId);
            if (post == null)
            {
                throw new Exception("Bai dang khong ton tai.");
            }

            if (post.CreatorUserId != currentUserId)
            {
                throw new Exception("Ban khong co quyen tu choi request cua bai dang nay.");
            }

            if (request.Status != 1)
            {
                throw new Exception("Request nay da duoc xu ly truoc do.");
            }

            request.Status = 3;
            request.DecidedAt = DateTime.Now;
            request.DecidedByUserId = currentUserId;

            _joinRequestRepository.Update(request);
            _joinRequestRepository.Save();

            _notificationService.NotifyRequestRejected(request);
        }
    }
}
