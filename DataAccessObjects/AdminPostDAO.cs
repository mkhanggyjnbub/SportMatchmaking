using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DataAccessObjects
{
    public class AdminPostDAO
    {
        private readonly SportMatchmakingContext _context;

        private const byte REPORT_TARGET_POST = 1;
        private const byte POST_STATUS_OPEN = 1;
        private const byte POST_STATUS_FULL = 2;
        private const byte POST_STATUS_COMPLETED = 4;
        private const byte POST_STATUS_CANCELLED = 5;

        public AdminPostDAO(SportMatchmakingContext context)
        {
            _context = context;
        }

        public async Task<List<MatchPost>> GetPostsAsync(
            string? keyword = null,
            int? sportId = null,
            byte? status = null,
            string? city = null,
            string? district = null,
            int? creatorUserId = null)
        {
            IQueryable<MatchPost> query = _context.Set<MatchPost>().AsQueryable();

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

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                city = city.Trim();
                query = query.Where(x => x.City != null && x.City.Contains(city));
            }

            if (!string.IsNullOrWhiteSpace(district))
            {
                district = district.Trim();
                query = query.Where(x => x.District != null && x.District.Contains(district));
            }

            if (creatorUserId.HasValue)
            {
                query = query.Where(x => x.CreatorUserId == creatorUserId.Value);
            }

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<MatchPost?> GetPostByIdAsync(long postId)
        {
            return await _context.Set<MatchPost>()
                .FirstOrDefaultAsync(x => x.PostId == postId);
        }

        public async Task<bool> CancelPostAsync(long postId)
        {
            var post = await _context.Set<MatchPost>()
                .FirstOrDefaultAsync(x => x.PostId == postId);

            if (post == null)
            {
                return false;
            }

            post.Status = POST_STATUS_CANCELLED;
            post.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePostStatusAsync(long postId, byte status)
        {
            var post = await _context.Set<MatchPost>()
                .FirstOrDefaultAsync(x => x.PostId == postId);

            if (post == null)
            {
                return false;
            }

            post.Status = status;
            post.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetReportCountByPostIdAsync(long postId)
        {
            return await _context.Set<Report>()
                .CountAsync(x => x.TargetType == REPORT_TARGET_POST && x.TargetPostId == postId);
        }

        public async Task<List<MatchPost>> GetHighlyReportedPostsAsync(int minReportCount = 1)
        {
            var reportedPostIds = await _context.Set<Report>()
                .Where(x => x.TargetType == REPORT_TARGET_POST && x.TargetPostId != null)
                .GroupBy(x => x.TargetPostId!.Value)
                .Where(g => g.Count() >= minReportCount)
                .Select(g => g.Key)
                .ToListAsync();

            return await _context.Set<MatchPost>()
                .Where(x => reportedPostIds.Contains(x.PostId))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> CountPostsAsync()
        {
            return await _context.Set<MatchPost>().CountAsync();
        }

        public async Task<int> CountCompletedPostsAsync()
        {
            return await _context.Set<MatchPost>()
                .CountAsync(x => x.Status == POST_STATUS_COMPLETED);
        }

        public async Task<List<SportPostCountStat>> GetPostCountBySportAsync()
        {
            return await _context.Set<Sport>()
                .Select(s => new SportPostCountStat
                {
                    SportId = s.SportId,
                    SportName = s.Name,
                    PostCount = s.MatchPosts.Count()
                })
                .OrderByDescending(x => x.PostCount)
                .ThenBy(x => x.SportName)
                .ToListAsync();
        }

        public async Task<List<SportPostCountStat>> GetPostCountBySportOpenOrFullAsync()
        {
            return await _context.Set<Sport>()
                .Select(s => new SportPostCountStat
                {
                    SportId = s.SportId,
                    SportName = s.Name,
                    PostCount = s.MatchPosts.Count(p => p.Status == POST_STATUS_OPEN || p.Status == POST_STATUS_FULL)
                })
                .OrderByDescending(x => x.PostCount)
                .ThenBy(x => x.SportName)
                .ToListAsync();
        }

        public async Task<Dictionary<byte, int>> GetPostCountByStatusAsync()
        {
            return await _context.Set<MatchPost>()
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<Dictionary<int, int>> GetWeeklyPostCountByYearAsync(int year)
        {
            var createdDates = await _context.Set<MatchPost>()
                .Where(x => x.CreatedAt.Year == year)
                .Select(x => x.CreatedAt)
                .ToListAsync();

            return createdDates
                .GroupBy(x => ISOWeek.GetWeekOfYear(x))
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<List<int>> GetAvailablePostYearsAsync()
        {
            return await _context.Set<MatchPost>()
                .Select(x => x.CreatedAt.Year)
                .Distinct()
                .OrderByDescending(x => x)
                .ToListAsync();
        }
    }
}