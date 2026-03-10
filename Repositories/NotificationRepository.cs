using BusinessObjects;
using Repositories.Interfaces;

namespace Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly SportMatchmakingContext _context;

        public NotificationRepository(SportMatchmakingContext context)
        {
            _context = context;
        }

        public void Add(Notification notification)
        {
            _context.Notifications.Add(notification);
        }

        public List<Notification> GetByUserId(int userId)
        {
            return _context.Notifications
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        public Notification? GetById(long notificationId)
        {
            return _context.Notifications.FirstOrDefault(x => x.NotificationId == notificationId);
        }

        public void Update(Notification notification)
        {
            _context.Notifications.Update(notification);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}