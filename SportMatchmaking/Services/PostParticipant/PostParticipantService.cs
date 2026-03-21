using BusinessObjects;
using Repositories.Post;
using Repositories.PostParticipant;

namespace Services.PostParticipant
{
    public class PostParticipantService : IPostParticipantService
    {
        private readonly IPostParticipantRepository _postParticipantRepository;
        private readonly IPostRepository _postRepository;

        public PostParticipantService(IPostParticipantRepository postParticipantRepository, IPostRepository postRepository)
        {
            _postParticipantRepository = postParticipantRepository;
            _postRepository = postRepository;
        }

        public async Task<List<BusinessObjects.PostParticipant>> GetParticipantsAsync(long postId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                return new List<BusinessObjects.PostParticipant>();
            }

            var host = _postParticipantRepository.GetByPostAndUser(postId, post.CreatorUserId);
            if (host == null)
            {
                _postParticipantRepository.Add(new BusinessObjects.PostParticipant
                {
                    PostId = postId,
                    UserId = post.CreatorUserId,
                    Role = (byte)ParticipantRole.Host,
                    Status = (byte)ParticipantStatus.Confirmed,
                    PartySize = 1,
                    JoinedAt = DateTime.Now,
                    LeftAt = null
                });

                await _postParticipantRepository.SaveAsync();
            }

            return await _postParticipantRepository.GetByPostIdAsync(postId);
        }

        public async Task<(bool Success, string Message)> LeavePostAsync(long postId, int userId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                return (false, "Bài ??ng không t?n t?i.");
            }

            var participant = _postParticipantRepository.GetByPostAndUser(postId, userId);
            if (participant == null)
            {
                return (false, "B?n ch?a tham gia bài ??ng này.");
            }

            if (participant.Role != (byte)ParticipantRole.Participant)
            {
                return (false, "Ch? kèo không th? r?i kèo theo cách này.");
            }

            if (participant.Status != (byte)ParticipantStatus.Confirmed)
            {
                return (false, "B?n không ? tr?ng thái ?ang tham gia.");
            }

            participant.Status = (byte)ParticipantStatus.Left;
            participant.LeftAt = DateTime.Now;

            _postParticipantRepository.Update(participant);
            await _postParticipantRepository.SaveAsync();

            await RecalculatePostStatusAsync(post);

            return (true, "R?i kèo thành công.");
        }

        public async Task<(bool Success, string Message)> MarkNoShowAsync(long postId, int targetUserId, int actorUserId, bool actorIsAdmin)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                return (false, "Bài ??ng không t?n t?i.");
            }

            bool isOwner = post.CreatorUserId == actorUserId;
            if (!actorIsAdmin && !isOwner)
            {
                return (false, "B?n không có quy?n ?ánh d?u no-show.");
            }

            if (post.StartTime > DateTime.Now)
            {
                return (false, "Ch? có th? ?ánh d?u no-show sau khi tr?n ?ã b?t ??u.");
            }

            var participant = _postParticipantRepository.GetByPostAndUser(postId, targetUserId);
            if (participant == null)
            {
                return (false, "Ng??i dùng này ch?a tham gia kèo.");
            }

            if (participant.Role != (byte)ParticipantRole.Participant)
            {
                return (false, "Không th? ?ánh d?u no-show cho ch? kèo.");
            }

            if (participant.Status != (byte)ParticipantStatus.Confirmed)
            {
                return (false, "Ch? có th? ?ánh d?u no-show cho ng??i ?ang ? tr?ng thái Confirmed.");
            }

            participant.Status = (byte)ParticipantStatus.NoShow;

            _postParticipantRepository.Update(participant);
            await _postParticipantRepository.SaveAsync();

            return (true, "?ã ?ánh d?u no-show.");
        }

        private async Task RecalculatePostStatusAsync(MatchPost post)
        {
            if (post.Status != (byte)PostStatus.Open && post.Status != (byte)PostStatus.Full)
            {
                return;
            }

            if (post.StartTime <= DateTime.Now)
            {
                return;
            }

            var confirmedSlots = await _postParticipantRepository.GetConfirmedParticipantSlotsAsync(post.PostId);

            var newStatus = confirmedSlots >= post.SlotsNeeded ? PostStatus.Full : PostStatus.Open;
            if (post.Status == (byte)newStatus)
            {
                return;
            }

            post.Status = (byte)newStatus;
            post.UpdatedAt = DateTime.Now;
            await _postRepository.UpdatePostAsync(post);
        }
    }
}
