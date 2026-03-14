using BusinessObjects;
using DataAccessObjects;
using System.Collections.Generic;

namespace Repositories.Notifications
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDAO _notificationDao;

        public NotificationRepository(NotificationDAO notificationDao)
        {
            _notificationDao = notificationDao;
        }

        public void Add(Notification entity)
        {
            _notificationDao.Add(entity);
        }

        public Notification? GetById(long notificationId, int userId)
        {
            return _notificationDao.GetById(notificationId, userId);
        }

        public List<Notification> GetRecentByUser(int userId, int take)
        {
            return _notificationDao.GetRecentByUser(userId, take);
        }

        public int GetUnreadCount(int userId)
        {
            return _notificationDao.GetUnreadCount(userId);
        }

        public void MarkAsRead(long notificationId, int userId)
        {
            _notificationDao.MarkAsRead(notificationId, userId);
        }

        public void MarkAllAsRead(int userId)
        {
            _notificationDao.MarkAllAsRead(userId);
        }

        public void Save()
        {
            _notificationDao.Save();
        }
    }
}

