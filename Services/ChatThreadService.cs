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

        private static string FormatRoomTime(DateTime sentAtLocal)
        {
            var now = DateTime.Now;

            if (sentAtLocal.Date == now.Date)
            {
                return sentAtLocal.ToString("HH:mm");
            }

            return sentAtLocal.ToString("dd/MM");
        }

        private static string FormatMessageSender(ChatMessage message)
        {
            if (message.SenderUser != null)
            {
                if (!string.IsNullOrWhiteSpace(message.SenderUser.UserName))
                {
                    return message.SenderUser.UserName;
                }

                if (!string.IsNullOrWhiteSpace(message.SenderUser.DisplayName))
                {
                    return message.SenderUser.DisplayName;
                }
            }

            return $"User {message.SenderUserId}";
        }

        private static string FormatThreadRoomName(ChatThread thread, MatchPost? post)
        {
            if (post != null && !string.IsNullOrWhiteSpace(post.Title))
            {
                return post.Title.Trim();
            }

            if (thread.PostId.HasValue)
            {
                return $"Bài đăng #{thread.PostId}";
            }

            return $"Phòng #{thread.ThreadId}";
        }

        public async Task<ChatIndexViewModel> GetChatIndexDataAsync(int currentUserId, long? threadId)
        {
            var threads = await _chatThreadRepository.GetThreadsByUserIdAsync(currentUserId);

            long? selectedThreadId = threadId;

            var roomItems = new List<ChatRoomItemViewModel>();

            foreach (var thread in threads)
            {
                var threadMessages = await _chatThreadRepository.GetMessagesByThreadIdAsync(thread.ThreadId);
                var lastMessage = threadMessages.LastOrDefault();

                var members = await _chatThreadRepository.GetThreadMembersByThreadIdAsync(thread.ThreadId);
                var memberCount = members.Count;

                MatchPost? post = null;
                if (thread.PostId.HasValue)
                {
                    post = await _chatThreadRepository.GetPostByIdAsync(thread.PostId.Value);
                }

                roomItems.Add(new ChatRoomItemViewModel
                {
                    ThreadId = thread.ThreadId,
                    PostId = thread.PostId,
                    RoomName = FormatThreadRoomName(thread, post),
                    LastMessage = lastMessage == null
                        ? "Chưa có tin nhắn"
                        : (lastMessage.IsDeleted == true
                            ? "Tin nhắn đã bị xóa"
                            : (lastMessage.MessageText ?? "")),
                    LastMessageTimeText = lastMessage != null
                        ? FormatRoomTime(lastMessage.SentAt.ToLocalTime())
                        : "",
                    MemberCount = memberCount,
                    HasUnread = false
                });
            }

            roomItems = roomItems
                .OrderByDescending(r =>
                {
                    var room = threads.First(t => t.ThreadId == r.ThreadId);
                    return room.CreatedAt;
                })
                .ToList();

            var roomLastMessageMap = new Dictionary<long, DateTime>();

            foreach (var room in roomItems)
            {
                var threadMessages = await _chatThreadRepository.GetMessagesByThreadIdAsync(room.ThreadId);
                var lastMessage = threadMessages.LastOrDefault();

                roomLastMessageMap[room.ThreadId] = lastMessage?.SentAt ?? DateTime.MinValue;
            }

            roomItems = roomItems
                .OrderByDescending(r => roomLastMessageMap[r.ThreadId])
                .ThenByDescending(r => threads.First(t => t.ThreadId == r.ThreadId).CreatedAt)
                .ToList();

            if (selectedThreadId == null && roomItems.Any())
            {
                selectedThreadId = roomItems.First().ThreadId;
            }

            foreach (var room in roomItems)
            {
                room.IsSelected = selectedThreadId.HasValue && room.ThreadId == selectedThreadId.Value;
                if (room.IsSelected)
                {
                    room.HasUnread = false;
                }
            }

            var selectedRoomTitle = "Chưa chọn phòng";
            var selectedRoomSubtitle = roomItems.Any()
                ? "Hãy chọn một phòng để xem nội dung chat"
                : "Bạn chưa tham gia phòng chat nào";
            var selectedRoomMemberCount = 0;

            var messageItems = new List<ChatMessageItemViewModel>();

            if (selectedThreadId.HasValue)
            {
                var selectedThread = await _chatThreadRepository.GetThreadByIdAsync(selectedThreadId.Value);

                if (selectedThread != null)
                {
                    MatchPost? selectedPost = null;
                    if (selectedThread.PostId.HasValue)
                    {
                        selectedPost = await _chatThreadRepository.GetPostByIdAsync(selectedThread.PostId.Value);
                    }

                    selectedRoomTitle = FormatThreadRoomName(selectedThread, selectedPost);

                    var selectedMembers = await _chatThreadRepository.GetThreadMembersByThreadIdAsync(selectedThreadId.Value);
                    selectedRoomMemberCount = selectedMembers.Count;

                    selectedRoomSubtitle = selectedThread.PostId.HasValue
                        ? $"Phòng chat theo bài đăng • {selectedRoomMemberCount} thành viên"
                        : $"Phòng chat trực tiếp • {selectedRoomMemberCount} thành viên";
                }
                    
                var messages = await _chatThreadRepository.GetMessagesByThreadIdAsync(selectedThreadId.Value);

                messageItems = messages.Select(m => new ChatMessageItemViewModel
                {
                    MessageId = m.MessageId,
                    ThreadId = m.ThreadId,
                    SenderUserId = m.SenderUserId,
                    SenderName = FormatMessageSender(m),
                    MessageText = m.IsDeleted == true ? "Tin nhắn đã bị xóa" : (m.MessageText ?? ""),
                    SentAt = m.SentAt,
                    EditedAt = m.EditedAt,
                    IsDeleted = m.IsDeleted,
                    IsMine = m.SenderUserId == currentUserId,
                    CanEditOrDelete = m.SenderUserId == currentUserId && m.IsDeleted != true
                }).ToList();
            }

            return new ChatIndexViewModel
            {
                CurrentUserId = currentUserId,
                SelectedThreadId = selectedThreadId,
                SelectedRoomTitle = selectedRoomTitle,
                SelectedRoomSubtitle = selectedRoomSubtitle,
                SelectedRoomMemberCount = selectedRoomMemberCount,
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

            var acceptedParticipants = await _chatThreadRepository.GetConfirmedParticipantsByPostIdAsync(postId);
            if (acceptedParticipants == null || !acceptedParticipants.Any())
            {
                return 0;
            }



            var thread = await EnsurePostGroupThreadCreatedAsync(postId);

            var existingMembers = await _chatThreadRepository.GetThreadMembersByThreadIdAsync(thread.ThreadId);
            var existingUserIds = existingMembers
                .Select(m => m.UserId)
                .ToHashSet();

            var usersToAdd = new List<int>();

            // luôn đảm bảo creator có trong phòng
            usersToAdd.Add(post.CreatorUserId);

            // thêm các participant đã accepted
            usersToAdd.AddRange(
                acceptedParticipants.Select(pp => pp.UserId)
            );

            var newMembers = usersToAdd
                .Distinct()
                .Where(userId => !existingUserIds.Contains(userId))
                .Select(userId => new ChatThreadMember
                {
                    ThreadId = thread.ThreadId,
                    UserId = userId,
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


        public async Task<long?> GetAccessiblePostThreadIdAsync(long postId, int currentUserId)
        {
            var post = await _chatThreadRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                return null;
            }

            var thread = await _chatThreadRepository.GetPostGroupThreadByPostIdAsync(postId);
            if (thread == null)
            {
                return null;
            }

            var members = await _chatThreadRepository.GetThreadMembersByThreadIdAsync(thread.ThreadId);
            var isMember = members.Any(x => x.UserId == currentUserId);

            return isMember ? thread.ThreadId : (long?)null;
        }

        public async Task<ChatMessage> SendMessageAsync(long threadId, int senderUserId, string messageText)
        {
            var thread = await _chatThreadRepository.GetThreadByIdAsync(threadId);
            if (thread == null)
            {
                throw new Exception("Thread does not exist.");
            }

            if (string.IsNullOrWhiteSpace(messageText))
            {
                throw new Exception("Message text cannot be empty.");
            }

            var members = await _chatThreadRepository.GetThreadMembersByThreadIdAsync(threadId);
            var isMember = members.Any(m => m.UserId == senderUserId);

            if (!isMember)
            {
                throw new Exception("Sender is not a member of this thread.");
            }

            var newMessage = new ChatMessage
            {
                ThreadId = threadId,
                SenderUserId = senderUserId,
                MessageText = messageText.Trim(),
                SentAt = DateTime.UtcNow,
                EditedAt = null,
                IsDeleted = false
            };

            await _chatThreadRepository.AddMessageAsync(newMessage);
            await _chatThreadRepository.SaveChangesAsync();

            var savedMessage = await _chatThreadRepository.GetMessageByIdAsync(newMessage.MessageId);
            return savedMessage ?? newMessage;
        }

        public async Task<List<int>> GetThreadMemberUserIdsAsync(long threadId)
        {
            var members = await _chatThreadRepository.GetThreadMembersByThreadIdAsync(threadId);
            return members
                .Select(m => m.UserId)
                .Distinct()
                .ToList();
        }

        public async Task<(bool success, string message)> EditMessageAsync(long messageId, long currentUserId, string newText)
        {
            if (string.IsNullOrWhiteSpace(newText))
            {
                return (false, "Nội dung tin nhắn không được để trống.");
            }

            var message = await _chatThreadRepository.GetMessageByIdAsync(messageId);
            if (message == null)
            {
                return (false, "Không tìm thấy tin nhắn.");
            }

            if (message.IsDeleted == true)
            {
                return (false, "Tin nhắn đã bị xóa, không thể chỉnh sửa.");
            }

            if (message.SenderUserId != currentUserId)
            {
                return (false, "Bạn không có quyền sửa tin nhắn này.");
            }

            message.MessageText = newText.Trim();
            message.EditedAt = DateTime.Now;

            _chatThreadRepository.UpdateMessage(message);
            await _chatThreadRepository.SaveChangesAsync();

            return (true, "Sửa tin nhắn thành công.");
        }

        public async Task<(bool success, string message)> DeleteMessageAsync(long messageId, long currentUserId)
        {
            var message = await _chatThreadRepository.GetMessageByIdAsync(messageId);
            if (message == null)
            {
                return (false, "Không tìm thấy tin nhắn.");
            }

            if (message.IsDeleted == true)
            {
                return (false, "Tin nhắn này đã được xóa trước đó.");
            }

            if (message.SenderUserId != currentUserId)
            {
                return (false, "Bạn không có quyền xóa tin nhắn này.");
            }

            message.IsDeleted = true;
            message.EditedAt = DateTime.Now;

            _chatThreadRepository.UpdateMessage(message);
            await _chatThreadRepository.SaveChangesAsync();

            return (true, "Xóa tin nhắn thành công.");
        }
    }
}
