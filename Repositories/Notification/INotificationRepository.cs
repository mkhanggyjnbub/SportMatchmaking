using BusinessObjects;
using System.Collections.Generic;

namespace Repositories.Notifications
{
    public interface INotificationRepository
    {
        void Add(Notification entity);
        Notification? GetById(long notificationId, int userId);
        List<Notification> GetRecentByUser(int userId, int take);
        int GetUnreadCount(int userId);
        void MarkAsRead(long notificationId, int userId);
        void MarkAllAsRead(int userId);
        void Save();
    }
}

