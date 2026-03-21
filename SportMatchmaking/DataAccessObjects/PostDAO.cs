using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public class PostDAO
    {
        private readonly SportMatchmakingContext _context;

        public PostDAO(SportMatchmakingContext context)
        {
            _context = context;
        }

        public async Task<List<MatchPost>> GetPostsAsync(string? keyword = null, int? sportId = null)
        {
            IQueryable<MatchPost> query = _context.Set<MatchPost>()
                .Include(x => x.Sport)
                    .ThenInclude(x => x.Image)
                .Include(x => x.CreatorUser);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(x =>
                    x.Title.Contains(keyword) ||
                    (x.Description != null && x.Description.Contains(keyword)) ||
                    (x.LocationText != null && x.LocationText.Contains(keyword)));
            }

            if (sportId.HasValue)
            {
                query = query.Where(x => x.SportId == sportId.Value);
            }

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<MatchPost?> GetPostByIdAsync(long postId)
        {
            return await _context.Set<MatchPost>()
                .Include(x => x.Sport)
                    .ThenInclude(x => x.Image)
                .Include(x => x.CreatorUser)
                .FirstOrDefaultAsync(x => x.PostId == postId);
        }

        public async Task<MatchPost> AddPostAsync(MatchPost post)
        {
            _context.Set<MatchPost>().Add(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task UpdatePostAsync(MatchPost post)
        {
            _context.Set<MatchPost>().Update(post);
            await _context.SaveChangesAsync();
        }
    }
}
