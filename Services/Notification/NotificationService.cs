using BusinessObjects;
using Repositories.JoinRequest;
using Repositories.Notifications;
using Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IJoinRequestRepository _joinRequestRepository;

        public NotificationService(
            INotificationRepository notificationRepository,
            IJoinRequestRepository joinRequestRepository)
        {
            _notificationRepository = notificationRepository;
            _joinRequestRepository = joinRequestRepository;
        }

        public void NotifyNewJoinRequest(BusinessObjects.JoinRequest request)
        {
            var post = _joinRequestRepository.GetPostById(request.PostId);
            if (post == null)
            {
                return;
            }

            var requester = _joinRequestRepository.GetUserById(request.RequesterUserId);
            if (requester == null)
            {
                return;
            }

            var title = "Có request mới vào bài của bạn";
            var requesterName = !string.IsNullOrWhiteSpace(requester.DisplayName)
                ? requester.DisplayName
                : requester.UserName;

            var body = $"{requesterName} đã gửi yêu cầu tham gia kèo \"{post.Title}\".";

            var data = new
            {
                postId = post.PostId,
                requestId = request.RequestId,
                type = "join_request_new"
            };

            var notification = new BusinessObjects.Notification
            {
                UserId = post.CreatorUserId,
                Type = "JoinRequest.New",
                Title = title,
                Body = body,
                DataJson = JsonSerializer.Serialize(data),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _notificationRepository.Add(notification);
            _notificationRepository.Save();
        }

        public void NotifyRequestAccepted(BusinessObjects.JoinRequest request)
        {
            NotifyRequestDecision(request, true);
        }

        public void NotifyRequestRejected(BusinessObjects.JoinRequest request)
        {
            NotifyRequestDecision(request, false);
        }

        private void NotifyRequestDecision(BusinessObjects.JoinRequest request, bool accepted)
        {
            var post = _joinRequestRepository.GetPostById(request.PostId);
            if (post == null)
            {
                return;
            }

            if (!request.DecidedByUserId.HasValue)
            {
                return;
            }

            var decider = _joinRequestRepository.GetUserById(request.DecidedByUserId.Value);
            if (decider == null)
            {
                return;
            }

            var deciderName = !string.IsNullOrWhiteSpace(decider.DisplayName)
                ? decider.DisplayName
                : decider.UserName;

            var title = accepted
                ? "Yêu cầu tham gia của bạn đã được chấp nhận"
                : "Yêu cầu tham gia của bạn đã bị từ chối";

            var body = accepted
                ? $"{deciderName} đã chấp nhận yêu cầu tham gia của bạn trong kèo \"{post.Title}\"."
                : $"{deciderName} đã từ chối yêu cầu tham gia của bạn trong kèo \"{post.Title}\".";

            var data = new
            {
                postId = post.PostId,
                requestId = request.RequestId,
                status = accepted ? "accepted" : "rejected",
                type = "join_request_decision"
            };

            var notification = new BusinessObjects.Notification
            {
                UserId = request.RequesterUserId,
                Type = accepted ? "JoinRequest.Accepted" : "JoinRequest.Rejected",
                Title = title,
                Body = body,
                DataJson = JsonSerializer.Serialize(data),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _notificationRepository.Add(notification);
            _notificationRepository.Save();
        }

        public List<NotificationItemDTO> GetRecentNotifications(int userId, int take = 10)
        {
            var items = _notificationRepository.GetRecentByUser(userId, take);

            return items.Select(x => new NotificationItemDTO
            {
                NotificationId = x.NotificationId,
                Title = x.Title,
                Body = x.Body,
                Type = x.Type,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt
            }).ToList();
        }

        public int GetUnreadCount(int userId)
        {
            return _notificationRepository.GetUnreadCount(userId);
        }

        public void MarkAllAsRead(int userId)
        {
            _notificationRepository.MarkAllAsRead(userId);
            _notificationRepository.Save();
        }

        public void DeleteAllNotifications(int userId)
        {
            _notificationRepository.DeleteAllForUser(userId);
            _notificationRepository.Save();
        }
    }
}

