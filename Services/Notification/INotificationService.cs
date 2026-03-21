using BusinessObjects;
using Services.DTOs;
using System.Collections.Generic;

namespace Services.Notifications
{
    public interface INotificationService
    {
        void NotifyNewJoinRequest(BusinessObjects.JoinRequest request);
        void NotifyRequestAccepted(BusinessObjects.JoinRequest request);
        void NotifyRequestRejected(BusinessObjects.JoinRequest request);

        List<NotificationItemDTO> GetRecentNotifications(int userId, int take = 10);
        int GetUnreadCount(int userId);
        void MarkAllAsRead(int userId);
    }
}

