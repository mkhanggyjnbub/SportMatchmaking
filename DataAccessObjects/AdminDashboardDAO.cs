using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public class AdminDashboardDAO
    {
        private readonly SportMatchmakingContext _context;

        private const byte POST_STATUS_COMPLETED = (byte)PostStatus.Completed;
        private const byte REPORT_STATUS_OPEN = (byte)ReportStatus.Open;

        public AdminDashboardDAO(SportMatchmakingContext context)
        {
            _context = context;
        }

        public async Task<int> CountUsersAsync()
        {
            return await _context.Set<AppUser>().CountAsync();
        }

        public async Task<int> CountBannedUsersAsync()
        {
            return await _context.Set<AppUser>()
                .CountAsync(x => x.IsBanned);
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

        public async Task<int> CountJoinRequestsAsync()
        {
            return await _context.Set<JoinRequest>().CountAsync();
        }

        public async Task<int> CountOpenReportsAsync()
        {
            return await _context.Set<Report>()
                .CountAsync(x => x.Status == REPORT_STATUS_OPEN);
        }

        public async Task<Dictionary<string, int>> GetDashboardSummaryAsync()
        {
            var result = new Dictionary<string, int>
            {
                { "TotalUsers", await CountUsersAsync() },
                { "BannedUsers", await CountBannedUsersAsync() },
                { "TotalPosts", await CountPostsAsync() },
                { "CompletedPosts", await CountCompletedPostsAsync() },
                { "TotalJoinRequests", await CountJoinRequestsAsync() },
                { "OpenReports", await CountOpenReportsAsync() }
            };

            return result;
        }
    }
}