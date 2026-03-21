

using DataAccessObjects;

namespace Repositories.Admin
{
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly AdminDashboardDAO _adminDashboardDAO;

        public AdminDashboardRepository(AdminDashboardDAO adminDashboardDAO)
        {
            _adminDashboardDAO = adminDashboardDAO;
        }

        public async Task<int> CountUsersAsync()
        {
            return await _adminDashboardDAO.CountUsersAsync();
        }

        public async Task<int> CountBannedUsersAsync()
        {
            return await _adminDashboardDAO.CountBannedUsersAsync();
        }

        public async Task<int> CountPostsAsync()
        {
            return await _adminDashboardDAO.CountPostsAsync();
        }

        public async Task<int> CountCompletedPostsAsync()
        {
            return await _adminDashboardDAO.CountCompletedPostsAsync();
        }

        public async Task<int> CountJoinRequestsAsync()
        {
            return await _adminDashboardDAO.CountJoinRequestsAsync();
        }

        public async Task<int> CountOpenReportsAsync()
        {
            return await _adminDashboardDAO.CountOpenReportsAsync();
        }

        public async Task<Dictionary<string, int>> GetDashboardSummaryAsync()
        {
            return await _adminDashboardDAO.GetDashboardSummaryAsync();
        }
    }
}