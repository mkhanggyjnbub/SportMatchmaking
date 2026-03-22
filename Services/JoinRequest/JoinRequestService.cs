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
                throw new Exception("Bai dang khong ton tai.");
            }

            var requester = _joinRequestRepository.GetUserById(dto.RequesterUserId);
            if (requester == null)
            {
                throw new Exception("Nguoi dung khong ton tai.");
            }

            if (post.CreatorUserId == dto.RequesterUserId)
            {
                throw new Exception("Ban khong the gui yeu cau vao bai dang cua chinh minh.");
            }

            var existingParticipant = _postParticipantRepository.GetByPostAndUser(dto.PostId, dto.RequesterUserId);
            if (existingParticipant != null && existingParticipant.Status == PostParticipantStatuses.Confirmed)
            {
                throw new Exception("Ban da tham gia bai dang nay roi, khong the gui request them.");
            }

            if (dto.SkillLevel.HasValue)
            {
                if (dto.SkillLevel.Value < 1 || dto.SkillLevel.Value > 10)
                {
                    throw new Exception("Ky nang cua ban phai tu 1 den 10.");
                }

                requester.SkillLevel = dto.SkillLevel.Value;
                requester.UpdatedAt = DateTime.Now;
            }

            var effectiveSkillLevel = dto.SkillLevel ?? requester.SkillLevel;
            var hasSkillRequirement = post.SkillMin.HasValue || post.SkillMax.HasValue;

            if (hasSkillRequirement && !effectiveSkillLevel.HasValue)
            {
                throw new Exception("Vui long nhap ky nang cua ban de gui request vao keo nay.");
            }

            if (effectiveSkillLevel.HasValue)
            {
                if (post.SkillMin.HasValue && effectiveSkillLevel.Value < post.SkillMin.Value)
                {
                    throw new Exception($"Trinh do cua ban chua dat muc toi thieu ({post.SkillMin}).");
                }

                if (post.SkillMax.HasValue && effectiveSkillLevel.Value > post.SkillMax.Value)
                {
                    throw new Exception($"Trinh do cua ban vuot qua muc toi da ({post.SkillMax}).");
                }
            }

            if (post.Status != (byte)PostStatus.Open)
            {
                throw new Exception("Bai dang hien khong o trang thai mo.");
            }

            if (post.ExpiresAt.HasValue && post.ExpiresAt.Value < DateTime.Now)
            {
                throw new Exception("Bai dang da het han.");
            }

            if (post.StartTime <= DateTime.Now)
            {
                throw new Exception("Keo nay da toi gio bat dau hoac da dien ra.");
            }

            if (dto.PartySize < 1 || dto.PartySize > 30)
            {
                throw new Exception("PartySize khong hop le.");
            }

            var existingPending = _joinRequestRepository.GetPendingRequest(dto.PostId, dto.RequesterUserId);
            if (existingPending != null)
            {
                throw new Exception("Ban da co mot yeu cau dang cho duyet o bai nay.");
            }

            var confirmedParticipantSlots = _joinRequestRepository.GetConfirmedParticipantSlots(dto.PostId);
            var remainingSlots = post.SlotsNeeded - confirmedParticipantSlots;

            if (remainingSlots <= 0)
            {
                throw new Exception("Bai dang da du nguoi.");
            }

            if (dto.PartySize > remainingSlots)
            {
                throw new Exception($"So cho con lai khong du. Hien chi con {remainingSlots} cho.");
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
                throw new Exception("Bai dang khong ton tai.");
            }

            if (post.CreatorUserId != currentUserId)
            {
                throw new Exception("Ban khong co quyen xem danh sach request cua bai dang nay.");
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
                throw new Exception("Request nay da duoc xu ly truoc do.");
            }

            if (post.Status != (byte)PostStatus.Open)
            {
                throw new Exception("Bai dang hien khong con mo.");
            }

            if (post.ExpiresAt.HasValue && post.ExpiresAt.Value < DateTime.Now)
            {
                throw new Exception("Bai dang da het han.");
            }

            var confirmedParticipantSlots = _joinRequestRepository.GetConfirmedParticipantSlots(post.PostId);
            var remainingSlots = post.SlotsNeeded - confirmedParticipantSlots;

            if (remainingSlots <= 0)
            {
                throw new Exception("Bai dang da du nguoi.");
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
