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

        private readonly INotificationRealtimeService _notificationRealtimeService;
        public NotificationService(
            INotificationRepository notificationRepository,
            IJoinRequestRepository joinRequestRepository,
            INotificationRealtimeService notificationRealtimeService)
        {
            _notificationRepository = notificationRepository;
            _joinRequestRepository = joinRequestRepository;
            _notificationRealtimeService = notificationRealtimeService;
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
            PushUnreadCountRealtime(post.CreatorUserId);
        }

        public void NotifyRequestAccepted(BusinessObjects.JoinRequest request)
        {
            NotifyRequestDecision(request, true);
        }

        public void NotifyRequestRejected(BusinessObjects.JoinRequest request)
        {
            NotifyRequestDecision(request, false);
        }

        public void NotifyReportResolved(BusinessObjects.Report report)
        {
            NotifyReportDecision(report, true);
        }

        public void NotifyReportDismissed(BusinessObjects.Report report)
        {
            NotifyReportDecision(report, false);
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
                
            PushUnreadCountRealtime(request.RequesterUserId);
        }

        private void NotifyReportDecision(BusinessObjects.Report report, bool resolved)
        {
            var title = resolved
                ? "Report của bạn đã được xử lí"
                : "Report của bạn đã bì từ chối";

            var body = resolved
                ? $"Admin da resolve report cua ban cho bai dang #{report.TargetPostId}."
                : $"Admin da dismiss report cua ban cho bai dang #{report.TargetPostId}.";

            var data = new
            {
                reportId = report.ReportId,
                postId = report.TargetPostId,
                status = resolved ? "resolved" : "dismissed",
                type = "report_decision"
            };

            var notification = new BusinessObjects.Notification
            {
                UserId = report.ReporterUserId,
                Type = resolved ? "Report.Resolved" : "Report.Dismissed",
                Title = title,
                Body = body,
                DataJson = JsonSerializer.Serialize(data),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _notificationRepository.Add(notification);
            _notificationRepository.Save();
            PushUnreadCountRealtime(report.ReporterUserId);
        }

        private void PushUnreadCountRealtime(int userId)
        {
            var unreadCount = _notificationRepository.GetUnreadCount(userId);
            _notificationRealtimeService.PushUnreadCountAsync(userId, unreadCount).Wait();
        }

        public List<NotificationItemDTO> GetRecentNotifications(int userId, int take = 10)
        {
            var items = _notificationRepository.GetRecentByUser(userId, take);

            return items.Select(x => new NotificationItemDTO
            {
                NotificationId = x.NotificationId,
                Title = x.Title,
                Body = x.Body,
                DataJson = x.DataJson,
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

