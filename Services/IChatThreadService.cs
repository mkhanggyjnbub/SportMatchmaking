//vinh

using BusinessObjects;
using Services.Models.Chat;

namespace Services
{
    public interface IChatThreadService
    {
        Task<ChatThread> EnsurePostGroupThreadCreatedAsync(long postId);

        Task<ChatIndexViewModel> GetChatIndexDataAsync(int currentUserId, long? threadId);
        Task<int> AddConfirmedParticipantsToThreadAsync(long postId);

        Task<ChatMessage> SendMessageAsync(long threadId, int senderUserId, string messageText);
    }
}