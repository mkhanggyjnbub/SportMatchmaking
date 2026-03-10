//vinh

using BusinessObjects;
using Repositories;
using Services.Models.Chat;

namespace Services
{
    public class ChatThreadService : IChatThreadService
    {
        private readonly IChatThreadRepository _chatThreadRepository;

        public ChatThreadService(IChatThreadRepository chatThreadRepository)
        {
            _chatThreadRepository = chatThreadRepository;
        }

        public async Task<ChatIndexViewModel> GetChatIndexDataAsync(int currentUserId, long? threadId)
        {
            var threads = await _chatThreadRepository.GetThreadsByUserIdAsync(currentUserId);

            var roomItems = new List<ChatRoomItemViewModel>();

            foreach (var thread in threads)
            {
                var threadMessages = await _chatThreadRepository.GetMessagesByThreadIdAsync(thread.ThreadId);
                var lastMessage = threadMessages.LastOrDefault();

                roomItems.Add(new ChatRoomItemViewModel
                {
                    ThreadId = thread.ThreadId,
                    PostId = thread.PostId,
                    RoomName = thread.PostId.HasValue
                        ? $"Bài đăng #{thread.PostId}"
                        : $"Phòng #{thread.ThreadId}",
                    LastMessage = lastMessage?.MessageText ?? "Chưa có tin nhắn",
                    LastMessageTimeText = lastMessage != null
                        ? lastMessage.SentAt.ToLocalTime().ToString("HH:mm")
                        : ""
                });
            }

            long? selectedThreadId = threadId;

            if (selectedThreadId == null && roomItems.Any())
            {
                selectedThreadId = roomItems.First().ThreadId;
            }

            var selectedRoomTitle = "Chưa chọn phòng";
            var messageItems = new List<ChatMessageItemViewModel>();

            if (selectedThreadId.HasValue)
            {
                var selectedThread = await _chatThreadRepository.GetThreadByIdAsync(selectedThreadId.Value);

                if (selectedThread != null)
                {
                    selectedRoomTitle = selectedThread.PostId.HasValue
                        ? $"Bài đăng #{selectedThread.PostId}"
                        : $"Phòng #{selectedThread.ThreadId}";
                }

                var messages = await _chatThreadRepository.GetMessagesByThreadIdAsync(selectedThreadId.Value);

                messageItems = messages.Select(m => new ChatMessageItemViewModel
                {
                    MessageId = m.MessageId,
                    ThreadId = m.ThreadId,
                    SenderUserId = m.SenderUserId,
                    SenderName = $"User {m.SenderUserId}",
                    MessageText = m.MessageText ?? "",
                    SentAt = m.SentAt,
                    IsMine = m.SenderUserId == currentUserId
                }).ToList();
            }

            return new ChatIndexViewModel
            {
                CurrentUserId = currentUserId,
                SelectedThreadId = selectedThreadId,
                SelectedRoomTitle = selectedRoomTitle,
                Rooms = roomItems,
                Messages = messageItems
            };
        }

        public async Task<ChatThread> EnsurePostGroupThreadCreatedAsync(long postId)
        {
            var post = await _chatThreadRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                throw new Exception("Post does not exist.");
            }

            var existingThread = await _chatThreadRepository.GetPostGroupThreadByPostIdAsync(postId);
            if (existingThread != null)
            {
                return existingThread;
            }

            var newThread = await _chatThreadRepository.CreatePostGroupThreadAsync(postId);
            await _chatThreadRepository.SaveChangesAsync();

            return newThread;
        }

        public async Task<int> AddConfirmedParticipantsToThreadAsync(long postId)
        {
            var post = await _chatThreadRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                throw new Exception("Post does not exist.");
            }

            var thread = await EnsurePostGroupThreadCreatedAsync(postId);

            var confirmedParticipants = await _chatThreadRepository.GetConfirmedParticipantsByPostIdAsync(postId);
            if (confirmedParticipants == null || !confirmedParticipants.Any())
            {
                return 0;
            }

            var existingMembers = await _chatThreadRepository.GetThreadMembersByThreadIdAsync(thread.ThreadId);
            var existingUserIds = existingMembers
                .Select(m => m.UserId)
                .ToHashSet();

            var newMembers = confirmedParticipants
                .Where(pp => !existingUserIds.Contains(pp.UserId))
                .Select(pp => new ChatThreadMember
                {
                    ThreadId = thread.ThreadId,
                    UserId = pp.UserId,
                    JoinedAt = DateTime.UtcNow,
                    IsMuted = false
                })
                .ToList();

            if (!newMembers.Any())
            {
                return 0;
            }

            await _chatThreadRepository.AddThreadMembersAsync(newMembers);
            await _chatThreadRepository.SaveChangesAsync();

            return newMembers.Count;
        }
    }
}