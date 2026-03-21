//vinh

using Microsoft.AspNetCore.SignalR;

namespace SportMatchmaking.Hubs
{
    public class ChatHub : Hub
    {
        public async Task JoinThread(string threadId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, threadId);
        }

        public async Task LeaveThread(string threadId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, threadId);
        }
    }
}