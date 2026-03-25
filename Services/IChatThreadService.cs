//vinh

using BusinessObjects;
using Services.Models.Chat;
using Services;

namespace Services
{
    public interface IChatThreadService
    {
        Task<ChatThread> EnsurePostGroupThreadCreatedAsync(long postId);

        Task<ChatIndexViewModel> GetChatIndexDataAsync(int currentUserId, long? threadId);
        Task<int> AddConfirmedParticipantsToThreadAsync(long postId);

        Task<long?> GetAccessiblePostThreadIdAsync(long postId, int currentUserId);

        Task<ChatMessage> SendMessageAsync(long threadId, int senderUserId, string messageText);
        Task<List<int>> GetThreadMemberUserIdsAsync(long threadId);

        Task<(bool success, string message)> EditMessageAsync(long messageId, long currentUserId, string newText);
        Task<(bool success, string message)> DeleteMessageAsync(long messageId, long currentUserId);
        Task<(bool success, string message)> DeleteRoomAsync(long threadId, int currentUserId);
        Task<(bool success, string message)> LeaveRoomAsync(long threadId, int currentUserId);
    }
}
