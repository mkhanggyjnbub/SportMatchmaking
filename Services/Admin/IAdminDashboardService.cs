namespace Services.Admin
{
    public interface IAdminDashboardService
    {
        Task<int> CountUsersAsync();
        Task<int> CountBannedUsersAsync();
        Task<int> CountPostsAsync();
        Task<int> CountCompletedPostsAsync();
        Task<int> CountJoinRequestsAsync();
        Task<int> CountOpenReportsAsync();
        Task<Dictionary<string, int>> GetDashboardSummaryAsync();
    }
}