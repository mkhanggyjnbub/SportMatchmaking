using BusinessObjects;
using Repositories.Interfaces;

namespace Repositories
{
    public class MatchPostRepository : IMatchPostRepository
    {
        private readonly SportMatchmakingContext _context;

        public MatchPostRepository(SportMatchmakingContext context)
        {
            _context = context;
        }

        public MatchPost? GetById(long postId)
        {
            return _context.MatchPosts.FirstOrDefault(x => x.PostId == postId);
        }

        public void Update(MatchPost post)
        {
            _context.MatchPosts.Update(post);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}