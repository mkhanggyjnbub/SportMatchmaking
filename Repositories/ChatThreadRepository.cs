//vinh

using BusinessObjects;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class ChatThreadRepository : IChatThreadRepository
    {
        private readonly SportMatchmakingContext _context;

        public ChatThreadRepository(SportMatchmakingContext context)
        {
            _context = context;
        }

        public async Task<MatchPost?> GetPostByIdAsync(long postId)
        {
            return await _context.MatchPosts
                .FirstOrDefaultAsync(p => p.PostId == postId);
        }

        public async Task<ChatThread?> GetPostGroupThreadByPostIdAsync(long postId)
        {
            return await _context.ChatThreads
                .FirstOrDefaultAsync(t => t.PostId == postId && t.ThreadType == 1);
        }

        public async Task<ChatThread> CreatePostGroupThreadAsync(long postId)
        {
            var thread = new ChatThread
            {
                ThreadType = 1,
                PostId = postId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.ChatThreads.AddAsync(thread);
            return thread;
        }


        public async Task<List<ChatThread>> GetThreadsByUserIdAsync(int userId)
        {
            return await _context.ChatThreadMembers
                .Where(m => m.UserId == userId)
                .Join(
                    _context.ChatThreads,
                    member => member.ThreadId,
                    thread => thread.ThreadId,
                    (member, thread) => thread
                )
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<ChatThread?> GetThreadByIdAsync(long threadId)
        {
            return await _context.ChatThreads
                .FirstOrDefaultAsync(t => t.ThreadId == threadId);
        }

        public async Task<List<ChatMessage>> GetMessagesByThreadIdAsync(long threadId)
        {
            return await _context.ChatMessages
                .Include(m => m.SenderUser)
                .Where(m => m.ThreadId == threadId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }


        public async Task<List<BusinessObjects.PostParticipant>> GetConfirmedParticipantsByPostIdAsync(long postId)
        {
            return await _context.PostParticipants
                .Where(pp => pp.PostId == postId
                          && pp.Status == PostParticipantStatuses.Confirmed)
                .ToListAsync();
        }

        public async Task<BusinessObjects.PostParticipant?> GetCreatorParticipantByPostIdAsync(long postId)
        {
            return await _context.PostParticipants
                .FirstOrDefaultAsync(pp => pp.PostId == postId && pp.Role == PostParticipantRoles.Creator);
        }

        public async Task<List<ChatThreadMember>> GetThreadMembersByThreadIdAsync(long threadId)
        {
            return await _context.ChatThreadMembers
                .Where(m => m.ThreadId == threadId)
                .ToListAsync();
        }

        public async Task<ChatThreadMember?> GetThreadMemberAsync(long threadId, int userId)
        {
            return await _context.ChatThreadMembers
                .FirstOrDefaultAsync(m => m.ThreadId == threadId && m.UserId == userId);
        }

        public async Task AddThreadMembersAsync(List<ChatThreadMember> members)
        {
            await _context.ChatThreadMembers.AddRangeAsync(members);
        }

        public void RemoveThreadMember(ChatThreadMember member)
        {
            _context.ChatThreadMembers.Remove(member);
        }

        public void RemoveThreadMembers(List<ChatThreadMember> members)
        {
            _context.ChatThreadMembers.RemoveRange(members);
        }

        public void RemoveMessages(List<ChatMessage> messages)
        {
            _context.ChatMessages.RemoveRange(messages);
        }

        public void RemoveThread(ChatThread thread)
        {
            _context.ChatThreads.Remove(thread);
        }

        public async Task AddMessageAsync(ChatMessage message)
        {
            await _context.ChatMessages.AddAsync(message);
        }

        public async Task<ChatMessage?> GetMessageByIdAsync(long messageId)
        {
            return await _context.ChatMessages
                .Include(m => m.SenderUser)
                .FirstOrDefaultAsync(m => m.MessageId == messageId);
        }

        public void UpdateMessage(ChatMessage message)
        {
            _context.ChatMessages.Update(message);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
