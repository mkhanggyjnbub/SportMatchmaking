using Azure.Core;
using Repositories.JoinRequest;
using Repositories.PostParticipant;
using Services.DTOs;
using Services.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.JoinRequest
{
    public class JoinRequestService : IJoinRequestService
    {
        private readonly IJoinRequestRepository _joinRequestRepository;
        private readonly IPostParticipantRepository _postParticipantRepository;
        private readonly INotificationService _notificationService;

        public JoinRequestService(
            IJoinRequestRepository joinRequestRepository,
            IPostParticipantRepository postParticipantRepository,
            INotificationService notificationService)
        {
            _joinRequestRepository = joinRequestRepository;
            _postParticipantRepository = postParticipantRepository;
            _notificationService = notificationService;
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
            if (existingParticipant != null && existingParticipant.Status == 1)
            {
                throw new Exception("Bạn đã tham gia bài đăng này rồi, không thể gửi request thêm.");
            }

            // Check skill
            if (requester.SkillLevel.HasValue)
            {
                if (post.SkillMin.HasValue && requester.SkillLevel.Value < post.SkillMin.Value)
                {
                    throw new Exception($"Trình độ của bạn chưa đạt mức tối thiểu ({post.SkillMin}).");
                }

                if (post.SkillMax.HasValue && requester.SkillLevel.Value > post.SkillMax.Value)
                {
                    throw new Exception($"Trình độ của bạn vượt quá mức tối đa ({post.SkillMax}).");
                }
            }

            // MatchPosts.Status = 1 là Open theo seed SQL
            if (post.Status != 1)
            {
                throw new Exception("Bài đăng hiện không ở trạng thái mở.");
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

            var existingPending = _joinRequestRepository.GetPendingRequest(dto.PostId, dto.RequesterUserId);
            if (existingPending != null)
            {
                throw new Exception("Bạn đã có một yêu cầu đang chờ duyệt ở bài này.");
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

            var entity = new BusinessObjects.JoinRequest
            {
                PostId = dto.PostId,
                RequesterUserId = dto.RequesterUserId,
                PartySize = dto.PartySize,
                Message = string.IsNullOrWhiteSpace(dto.Message) ? null : dto.Message.Trim(),
                GuestNames = string.IsNullOrWhiteSpace(dto.GuestNames) ? null : dto.GuestNames.Trim(),
                Status = 1, // 1 = Pending
                CreatedAt = DateTime.Now,
                DecidedAt = null,
                DecidedByUserId = null
            };

            _joinRequestRepository.Add(entity);
            _joinRequestRepository.Save();

            // Thông báo cho chủ bài viết có request mới
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
                PostTitle = x.Post != null ? x.Post.Title : "(Không có tiêu đề)",
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
                throw new Exception("Request không tồn tại.");
            }

            if (request.RequesterUserId != currentUserId)
            {
                throw new Exception("Bạn không có quyền hủy request này.");
            }

            if (request.Status != 1)
            {
                throw new Exception("Chỉ có thể hủy request đang ở trạng thái Pending.");
            }

            request.Status = 4; // Cancelled

            _joinRequestRepository.Update(request);
            _joinRequestRepository.Save();

            // Thông báo cho người gửi request là đã được chấp nhận
            _notificationService.NotifyRequestAccepted(request);
        }
        public void AcceptRequest(long requestId, int currentUserId)
        {
            var request = _joinRequestRepository.GetById(requestId);
            if (request == null)
            {
                throw new Exception("Request không tồn tại.");
            }

            var post = request.Post ?? _joinRequestRepository.GetPostById(request.PostId);
            if (post == null)
            {
                throw new Exception("Bài đăng không tồn tại.");
            }

            if (post.CreatorUserId != currentUserId)
            {
                throw new Exception("Bạn không có quyền duyệt request của bài đăng này.");
            }

            if (request.Status != 1)
            {
                throw new Exception("Request này đã được xử lý trước đó.");
            }

            // 1 = Open
            if (post.Status != 1)
            {
                throw new Exception("Bài đăng hiện không còn mở.");
            }

            if (post.ExpiresAt.HasValue && post.ExpiresAt.Value < DateTime.Now)
            {
                throw new Exception("Bài đăng đã hết hạn.");
            }

            var confirmedParticipantSlots = _joinRequestRepository.GetConfirmedParticipantSlots(post.PostId);
            var remainingSlots = post.SlotsNeeded - confirmedParticipantSlots;

            if (remainingSlots <= 0)
            {
                throw new Exception("Bài đăng đã đủ người.");
            }

            if (request.PartySize > remainingSlots)
            {
                throw new Exception($"Không đủ slot để duyệt request này. Chỉ còn {remainingSlots} chỗ.");
            }

            request.Status = 2; // Accepted
            request.DecidedAt = DateTime.Now;
            request.DecidedByUserId = currentUserId;

            var participant = _postParticipantRepository.GetByPostAndUser(post.PostId, request.RequesterUserId);

            if (participant == null)
            {
                participant = new BusinessObjects.PostParticipant
                {
                    PostId = post.PostId,
                    UserId = request.RequesterUserId,
                    Role = 2,      // Participant
                    Status = 1,    // Confirmed
                    PartySize = request.PartySize,
                    JoinedAt = DateTime.Now,
                    LeftAt = null
                };

                _postParticipantRepository.Add(participant);
            }
            else
            {
                participant.Role = 2;
                participant.Status = 1;
                participant.PartySize = request.PartySize;
                participant.LeftAt = null;

                if (participant.JoinedAt == default)
                {
                    participant.JoinedAt = DateTime.Now;
                }

                _postParticipantRepository.Update(participant);
            }

            // Tính lại slot sau khi vừa accept xong
            var updatedConfirmedParticipantSlots =
                _joinRequestRepository.GetConfirmedParticipantSlots(post.PostId) + request.PartySize;

            if (updatedConfirmedParticipantSlots >= post.SlotsNeeded)
            {
                post.Status = 2; // Full
                post.UpdatedAt = DateTime.Now;
                _joinRequestRepository.UpdatePost(post);
            }
            else
            {
                post.Status = 1; // vẫn Open
                post.UpdatedAt = DateTime.Now;
                _joinRequestRepository.UpdatePost(post);
            }

            _joinRequestRepository.Update(request);
            _joinRequestRepository.Save();

            // Thông báo cho người gửi request là đã bị từ chối
            _notificationService.NotifyRequestRejected(request);
        }

        public void RejectRequest(long requestId, int currentUserId)
        {
            var request = _joinRequestRepository.GetById(requestId);
            if (request == null)
            {
                throw new Exception("Request không tồn tại.");
            }

            var post = request.Post ?? _joinRequestRepository.GetPostById(request.PostId);
            if (post == null)
            {
                throw new Exception("Bài đăng không tồn tại.");
            }

            if (post.CreatorUserId != currentUserId)
            {
                throw new Exception("Bạn không có quyền từ chối request của bài đăng này.");
            }

            if (request.Status != 1)
            {
                throw new Exception("Request này đã được xử lý trước đó.");
            }

            request.Status = 3; // Rejected
            request.DecidedAt = DateTime.Now;
            request.DecidedByUserId = currentUserId;

            _joinRequestRepository.Update(request);
            _joinRequestRepository.Save();
        }
    }
}
