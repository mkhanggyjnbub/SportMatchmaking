using Microsoft.AspNetCore.SignalR;

namespace SportMatchmaking.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinUserGroup(string userId)
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
        }

        public async Task LeaveUserGroup(string userId)
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
        }

        public async Task NotifyUser(string userId, string title, string message, string type = "info")
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", new
                {
                    title = title,
                    message = message,
                    type = type,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        public async Task BroadcastNotification(string title, string message, string type = "info")
        {
            await Clients.All.SendAsync("ReceiveNotification", new
            {
                title = title,
                message = message,
                type = type,
                timestamp = DateTime.UtcNow
            });
        }
    }
}