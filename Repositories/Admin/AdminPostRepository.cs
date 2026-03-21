using BusinessObjects;
using DataAccessObjects;

namespace Repositories.Admin
{
    public class AdminPostRepository : IAdminPostRepository
    {
        private readonly AdminPostDAO _adminPostDAO;

        public AdminPostRepository(AdminPostDAO adminPostDAO)
        {
            _adminPostDAO = adminPostDAO;
        }

        public async Task<List<MatchPost>> GetPostsAsync(
            string? keyword = null,
            int? sportId = null,
            byte? status = null,
            string? city = null,
            string? district = null,
            int? creatorUserId = null)
        {
            return await _adminPostDAO.GetPostsAsync(keyword, sportId, status, city, district, creatorUserId);
        }

        public async Task<MatchPost?> GetPostByIdAsync(long postId)
        {
            return await _adminPostDAO.GetPostByIdAsync(postId);
        }

        public async Task<bool> CancelPostAsync(long postId)
        {
            return await _adminPostDAO.CancelPostAsync(postId);
        }

        public async Task<bool> UpdatePostStatusAsync(long postId, byte status)
        {
            return await _adminPostDAO.UpdatePostStatusAsync(postId, status);
        }

        public async Task<int> GetReportCountByPostIdAsync(long postId)
        {
            return await _adminPostDAO.GetReportCountByPostIdAsync(postId);
        }

        public async Task<List<MatchPost>> GetHighlyReportedPostsAsync(int minReportCount = 1)
        {
            return await _adminPostDAO.GetHighlyReportedPostsAsync(minReportCount);
        }

        public async Task<int> CountPostsAsync()
        {
            return await _adminPostDAO.CountPostsAsync();
        }

        public async Task<int> CountCompletedPostsAsync()
        {
            return await _adminPostDAO.CountCompletedPostsAsync();
        }

        public async Task<List<SportPostCountStat>> GetPostCountBySportAsync()
        {
            return await _adminPostDAO.GetPostCountBySportAsync();
        }

        public async Task<List<SportPostCountStat>> GetPostCountBySportOpenOrFullAsync()
        {
            return await _adminPostDAO.GetPostCountBySportOpenOrFullAsync();
        }

        public async Task<Dictionary<byte, int>> GetPostCountByStatusAsync()
        {
            return await _adminPostDAO.GetPostCountByStatusAsync();
        }

        public async Task<Dictionary<int, int>> GetWeeklyPostCountByYearAsync(int year)
        {
            return await _adminPostDAO.GetWeeklyPostCountByYearAsync(year);
        }

        public async Task<List<int>> GetAvailablePostYearsAsync()
        {
            return await _adminPostDAO.GetAvailablePostYearsAsync();
        }
    }
}