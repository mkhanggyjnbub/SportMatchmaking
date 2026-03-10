using BusinessObjects;
using Repositories.Interfaces;

namespace Repositories
{
    public class PostParticipantRepository : IPostParticipantRepository
    {
        private readonly SportMatchmakingContext _context;

        public PostParticipantRepository(SportMatchmakingContext context)
        {
            _context = context;
        }

        public List<PostParticipant> GetConfirmedParticipantsByPostId(long postId)
        {
            return _context.PostParticipants
                .Where(x => x.PostId == postId && x.Status == 1)
                .ToList();
        }

        public PostParticipant? GetByPostAndUser(long postId, int userId)
        {
            return _context.PostParticipants
                .FirstOrDefault(x => x.PostId == postId && x.UserId == userId);
        }

        public void Add(PostParticipant participant)
        {
            _context.PostParticipants.Add(participant);
        }

        public void Update(PostParticipant participant)
        {
            _context.PostParticipants.Update(participant);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}