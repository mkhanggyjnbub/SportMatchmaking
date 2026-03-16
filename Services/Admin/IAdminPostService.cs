using BusinessObjects;

namespace Services.Admin
{
    public interface IAdminPostService
    {
        Task<List<MatchPost>> GetPostsAsync(
            string? keyword = null,
            int? sportId = null,
            byte? status = null,
            string? city = null,
            string? district = null,
            int? creatorUserId = null);

        Task<MatchPost?> GetPostByIdAsync(long postId);
        Task<(bool Success, string Message)> CancelPostAsync(long postId);
        Task<(bool Success, string Message)> UpdatePostStatusAsync(long postId, byte status);
        Task<int> GetReportCountByPostIdAsync(long postId);
        Task<List<MatchPost>> GetHighlyReportedPostsAsync(int minReportCount = 1);

        Task<int> CountPostsAsync();
        Task<int> CountCompletedPostsAsync();

        Task<List<SportPostCountStat>> GetPostCountBySportAsync();
        Task<List<SportPostCountStat>> GetPostCountBySportOpenOrFullAsync();
        Task<Dictionary<byte, int>> GetPostCountByStatusAsync();
        Task<Dictionary<int, int>> GetWeeklyPostCountByYearAsync(int year);
        Task<List<int>> GetAvailablePostYearsAsync();
    }
}