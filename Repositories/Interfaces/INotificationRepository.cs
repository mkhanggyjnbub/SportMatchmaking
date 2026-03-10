using BusinessObjects;

namespace Repositories.Interfaces
{
    public interface INotificationRepository
    {
        void Add(Notification notification);
        List<Notification> GetByUserId(int userId);
        Notification? GetById(long notificationId);
        void Update(Notification notification);
        void Save();
    }
}