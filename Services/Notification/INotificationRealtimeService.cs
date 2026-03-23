using System.Threading.Tasks;

namespace Services.Notifications
{
    public interface INotificationRealtimeService
    {
        Task PushUnreadCountAsync(int userId, int unreadCount);
    }
}