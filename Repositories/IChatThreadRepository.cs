using BusinessObjects;

namespace Repositories
{
    public interface IChatThreadRepository
    {
        Task<MatchPost?> GetPostByIdAsync(long postId);
        Task<ChatThread?> GetPostGroupThreadByPostIdAsync(long postId);
        Task<ChatThread> CreatePostGroupThreadAsync(long postId);

        Task<List<ChatThread>> GetThreadsByUserIdAsync(int userId);
        Task<ChatThread?> GetThreadByIdAsync(long threadId);
        Task<List<ChatMessage>> GetMessagesByThreadIdAsync(long threadId);
        Task<List<PostParticipant>> GetConfirmedParticipantsByPostIdAsync(long postId);
        Task<PostParticipant?> GetCreatorParticipantByPostIdAsync(long postId);
        Task<List<ChatThreadMember>> GetThreadMembersByThreadIdAsync(long threadId);
        Task AddThreadMembersAsync(List<ChatThreadMember> members);

        Task AddMessageAsync(ChatMessage message);
        Task SaveChangesAsync();
    }
}