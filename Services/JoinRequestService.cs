using BusinessObjects;
using Repositories.Interfaces;
using Services.Helpers;
using Services.Interfaces;
using Services.Models.JoinRequest;

namespace Services
{
    public class JoinRequestService : IJoinRequestService
    {
        private readonly IJoinRequestRepository _joinRequestRepository;
        private readonly IMatchPostRepository _matchPostRepository;
        private readonly IPostParticipantRepository _postParticipantRepository;
        private readonly INotificationService _notificationService;

        public JoinRequestService(
            IJoinRequestRepository joinRequestRepository,
            IMatchPostRepository matchPostRepository,
            IPostParticipantRepository postParticipantRepository,
            INotificationService notificationService)
        {
            _joinRequestRepository = joinRequestRepository;
            _matchPostRepository = matchPostRepository;
            _postParticipantRepository = postParticipantRepository;
            _notificationService = notificationService;
        }

        public (bool Success, string Message) CreateRequest(int requesterUserId, CreateJoinRequestModel model)
        {
            var post = _matchPostRepository.GetById(model.PostId);
            if (post == null)
                return (false, "Bài đăng không tồn tại.");

            if (post.CreatorUserId == requesterUserId)
                return (false, "Bạn không thể tự gửi request vào bài của mình.");

            if (post.Status != 1)
                return (false, "Bài đăng hiện không còn mở.");

            if (post.ExpiresAt.HasValue && post.ExpiresAt.Value <= DateTime.Now)
                return (false, "Bài đăng đã hết hạn.");

            var existingPending = _joinRequestRepository.GetPendingRequest(model.PostId, requesterUserId);
            if (existingPending != null)
                return (false, "Bạn đã có một request đang chờ ở bài này.");

            var request = new JoinRequest
            {
                PostId = model.PostId,
                RequesterUserId = requesterUserId,
                PartySize = model.PartySize,
                Message = model.Message,
                GuestNames = model.GuestNames,
                Status = 1,
                CreatedAt = DateTime.Now
            };

            _joinRequestRepository.Add(request);
            _joinRequestRepository.Save();

            _notificationService.CreateNotification(
                post.CreatorUserId,
                "JoinRequestCreated",
                "Có yêu cầu tham gia mới",
                $"Bài đăng #{post.PostId} vừa có request mới với số lượng {model.PartySize} người."
            );

            return (true, "Gửi yêu cầu tham gia thành công.");
        }

        public List<JoinRequestListItemModel> GetRequestsByPostId(long postId, int currentUserId)
        {
            var post = _matchPostRepository.GetById(postId);
            if (post == null || post.CreatorUserId != currentUserId)
                return new List<JoinRequestListItemModel>();

            return _joinRequestRepository.GetRequestsByPostId(postId)
                .Select(x => new JoinRequestListItemModel
                {
                    RequestId = x.RequestId,
                    PostId = x.PostId,
                    RequesterUserId = x.RequesterUserId,
                    RequesterName = x.RequesterUser?.DisplayName
                                    ?? x.RequesterUser?.UserName
                                    ?? $"User {x.RequesterUserId}",
                    PartySize = x.PartySize,
                    Message = x.Message,
                    GuestNames = x.GuestNames,
                    Status = x.Status,
                    StatusText = StatusMapper.JoinRequestStatus(x.Status),
                    CreatedAt = x.CreatedAt,
                    DecidedAt = x.DecidedAt,
                    DecidedByUserId = x.DecidedByUserId
                })
                .ToList();
        }

        public List<JoinRequestListItemModel> GetMyRequests(int requesterUserId)
        {
            return _joinRequestRepository.GetRequestsByRequesterId(requesterUserId)
                .Select(x => new JoinRequestListItemModel
                {
                    RequestId = x.RequestId,
                    PostId = x.PostId,
                    RequesterUserId = x.RequesterUserId,
                    RequesterName = x.RequesterUser?.DisplayName
                                    ?? x.RequesterUser?.UserName
                                    ?? $"User {x.RequesterUserId}",
                    PartySize = x.PartySize,
                    Message = x.Message,
                    GuestNames = x.GuestNames,
                    Status = x.Status,
                    StatusText = StatusMapper.JoinRequestStatus(x.Status),
                    CreatedAt = x.CreatedAt,
                    DecidedAt = x.DecidedAt,
                    DecidedByUserId = x.DecidedByUserId
                })
                .ToList();
        }

        public (bool Success, string Message) CancelRequest(long requestId, int requesterUserId)
        {
            var request = _joinRequestRepository.GetById(requestId);
            if (request == null)
                return (false, "Request không tồn tại.");

            if (request.RequesterUserId != requesterUserId)
                return (false, "Bạn không có quyền hủy request này.");

            if (request.Status != 1)
                return (false, "Chỉ request đang Pending mới được hủy.");

            request.Status = 4;
            request.DecidedAt = DateTime.Now;
            request.DecidedByUserId = requesterUserId;

            _joinRequestRepository.Update(request);
            _joinRequestRepository.Save();

            var post = _matchPostRepository.GetById(request.PostId);
            if (post != null)
            {
                _notificationService.CreateNotification(
                    post.CreatorUserId,
                    "JoinRequestCancelled",
                    "Người chơi đã hủy yêu cầu tham gia",
                    $"Request #{request.RequestId} vào bài #{request.PostId} đã bị hủy."
                );
            }

            return (true, "Đã hủy request thành công.");
        }

        public (bool Success, string Message) ReviewRequest(int ownerUserId, ReviewJoinRequestModel model)
        {
            var request = _joinRequestRepository.GetById(model.RequestId);
            if (request == null)
                return (false, "Request không tồn tại.");

            var post = _matchPostRepository.GetById(request.PostId);
            if (post == null)
                return (false, "Bài đăng không tồn tại.");

            if (post.CreatorUserId != ownerUserId)
                return (false, "Bạn không phải chủ bài đăng.");

            if (request.Status != 1)
                return (false, "Request này đã được xử lý trước đó.");

            if (post.Status != 1)
                return (false, "Bài đăng không còn ở trạng thái mở.");

            if (post.ExpiresAt.HasValue && post.ExpiresAt.Value <= DateTime.Now)
                return (false, "Bài đăng đã hết hạn.");

            if (model.IsAccept)
            {
                var confirmedParticipants = _postParticipantRepository.GetConfirmedParticipantsByPostId(post.PostId);
                var currentSlots = confirmedParticipants.Sum(x => x.PartySize);
                var maxSlots = post.SlotsNeeded;

                if (currentSlots + request.PartySize > maxSlots)
                    return (false, "Không đủ slot trống để accept request này.");

                request.Status = 2;
                request.DecidedAt = DateTime.Now;
                request.DecidedByUserId = ownerUserId;
                _joinRequestRepository.Update(request);

                var existingParticipant = _postParticipantRepository.GetByPostAndUser(post.PostId, request.RequesterUserId);
                if (existingParticipant == null)
                {
                    var participant = new PostParticipant
                    {
                        PostId = post.PostId,
                        UserId = request.RequesterUserId,
                        Role = 2,
                        Status = 1,
                        PartySize = request.PartySize,
                        JoinedAt = DateTime.Now
                    };

                    _postParticipantRepository.Add(participant);
                }
                else
                {
                    existingParticipant.Status = 1;
                    existingParticipant.Role = 2;
                    existingParticipant.PartySize = request.PartySize;
                    existingParticipant.JoinedAt = DateTime.Now;

                    _postParticipantRepository.Update(existingParticipant);
                }

                var afterAcceptedSlots = currentSlots + request.PartySize;
                if (afterAcceptedSlots >= maxSlots)
                {
                    post.Status = 2;
                    _matchPostRepository.Update(post);
                }

                _joinRequestRepository.Save();
                _postParticipantRepository.Save();
                _matchPostRepository.Save();

                _notificationService.CreateNotification(
                    request.RequesterUserId,
                    "JoinRequestAccepted",
                    "Yêu cầu tham gia đã được chấp nhận",
                    $"Request #{request.RequestId} của bạn vào bài #{post.PostId} đã được chấp nhận."
                );

                return (true, "Đã accept request thành công.");
            }
            else
            {
                request.Status = 3;
                request.DecidedAt = DateTime.Now;
                request.DecidedByUserId = ownerUserId;

                _joinRequestRepository.Update(request);
                _joinRequestRepository.Save();

                _notificationService.CreateNotification(
                    request.RequesterUserId,
                    "JoinRequestRejected",
                    "Yêu cầu tham gia đã bị từ chối",
                    $"Request #{request.RequestId} của bạn vào bài #{post.PostId} đã bị từ chối."
                );

                return (true, "Đã reject request thành công.");
            }
        }
    }
}