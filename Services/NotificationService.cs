using BusinessObjects;
using Repositories.Interfaces;
using Services.Interfaces;
using Services.Models.Notification;

namespace Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public void CreateNotification(int userId, string type, string title, string? body, string? dataJson = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Body = body,
                DataJson = dataJson,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _notificationRepository.Add(notification);
            _notificationRepository.Save();
        }

        public List<NotificationListItemModel> GetMyNotifications(int userId)
        {
            return _notificationRepository.GetByUserId(userId)
                .Select(x => new NotificationListItemModel
                {
                    NotificationId = x.NotificationId,
                    UserId = x.UserId,
                    Type = x.Type,
                    Title = x.Title,
                    Body = x.Body,
                    IsRead = x.IsRead,
                    CreatedAt = x.CreatedAt
                })
                .ToList();
        }

        public bool MarkAsRead(long notificationId, int userId)
        {
            var notification = _notificationRepository.GetById(notificationId);
            if (notification == null || notification.UserId != userId)
                return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.Now;

            _notificationRepository.Update(notification);
            _notificationRepository.Save();
            return true;
        }
    }
}