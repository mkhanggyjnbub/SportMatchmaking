using Services.Models.Notification;

namespace Services.Interfaces
{
    public interface INotificationService
    {
        void CreateNotification(int userId, string type, string title, string? body, string? dataJson = null);
        List<NotificationListItemModel> GetMyNotifications(int userId);
        bool MarkAsRead(long notificationId, int userId);
    }
}