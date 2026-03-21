using Repositories.Admin;

namespace Services.Admin
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IAdminDashboardRepository _adminDashboardRepository;

        public AdminDashboardService(IAdminDashboardRepository adminDashboardRepository)
        {
            _adminDashboardRepository = adminDashboardRepository;
        }

        public async Task<int> CountUsersAsync()
        {
            return await _adminDashboardRepository.CountUsersAsync();
        }

        public async Task<int> CountBannedUsersAsync()
        {
            return await _adminDashboardRepository.CountBannedUsersAsync();
        }

        public async Task<int> CountPostsAsync()
        {
            return await _adminDashboardRepository.CountPostsAsync();
        }

        public async Task<int> CountCompletedPostsAsync()
        {
            return await _adminDashboardRepository.CountCompletedPostsAsync();
        }

        public async Task<int> CountJoinRequestsAsync()
        {
            return await _adminDashboardRepository.CountJoinRequestsAsync();
        }

        public async Task<int> CountOpenReportsAsync()
        {
            return await _adminDashboardRepository.CountOpenReportsAsync();
        }

        public async Task<Dictionary<string, int>> GetDashboardSummaryAsync()
        {
            return await _adminDashboardRepository.GetDashboardSummaryAsync();
        }
    }
}