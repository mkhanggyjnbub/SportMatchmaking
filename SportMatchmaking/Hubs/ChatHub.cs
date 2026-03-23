//vinh

using Microsoft.AspNetCore.SignalR;

namespace SportMatchmaking.Hubs
{
    public class ChatHub : Hub
    {
        public async Task JoinThread(string threadId)
        {
            if (!string.IsNullOrWhiteSpace(threadId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, threadId);
            }
        }

        public async Task LeaveThread(string threadId)
        {
            if (!string.IsNullOrWhiteSpace(threadId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, threadId);
            }
        }

        public async Task SendMessage(string threadId, long messageId, string senderName, string messageText, string senderAvatarInitial)
        {
            if (!string.IsNullOrWhiteSpace(threadId))
            {
                await Clients.Group(threadId).SendAsync("ReceiveMessage", new
                {
                    messageId = messageId,
                    senderName = senderName,
                    messageText = messageText,
                    senderAvatarInitial = senderAvatarInitial,
                    sentAt = DateTime.UtcNow,
                    isDeleted = false
                });
            }
        }

        public async Task EditMessage(string threadId, long messageId, string newText)
        {
            if (!string.IsNullOrWhiteSpace(threadId))
            {
                await Clients.Group(threadId).SendAsync("MessageEdited", new
                {
                    messageId = messageId,
                    newText = newText,
                    editedAt = DateTime.UtcNow
                });
            }
        }

        public async Task DeleteMessage(string threadId, long messageId)
        {
            if (!string.IsNullOrWhiteSpace(threadId))
            {
                await Clients.Group(threadId).SendAsync("MessageDeleted", new
                {
                    messageId = messageId
                });
            }
        }
    }
}