using Microsoft.AspNetCore.SignalR;
using Services.Notifications;
using SportMatchmaking.Hubs;

namespace SportMatchmaking.Services
{
    public class NotificationRealtimeService : INotificationRealtimeService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationRealtimeService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task PushUnreadCountAsync(int userId, int unreadCount)
        {
            await _hubContext.Clients
                .Group($"user_{userId}")
                .SendAsync("ReceiveNotification", new
                {
                    userId,
                    unreadCount
                });
        }
    }
}