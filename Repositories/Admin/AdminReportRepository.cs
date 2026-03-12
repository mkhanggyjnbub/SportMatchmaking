using BusinessObjects;
using DataAccessObjects;

namespace Repositories.Admin
{
    public class AdminReportRepository : IAdminReportRepository
    {
        private readonly AdminReportDAO _adminReportDAO;

        public AdminReportRepository(AdminReportDAO adminReportDAO)
        {
            _adminReportDAO = adminReportDAO;
        }

        public async Task<List<Report>> GetReportsAsync(
            byte? status = null,
            byte? targetType = null,
            byte? reasonCode = null,
            int? reporterUserId = null)
        {
            return await _adminReportDAO.GetReportsAsync(status, targetType, reasonCode, reporterUserId);
        }

        public async Task<Report?> GetReportByIdAsync(long reportId)
        {
            return await _adminReportDAO.GetReportByIdAsync(reportId);
        }

        public async Task<bool> UpdateReportStatusAsync(long reportId, byte status, int reviewedByUserId, string? resolution = null)
        {
            return await _adminReportDAO.UpdateReportStatusAsync(reportId, status, reviewedByUserId, resolution);
        }

        public async Task<bool> ResolveReportAsync(long reportId, int reviewedByUserId, string? resolution)
        {
            return await _adminReportDAO.ResolveReportAsync(reportId, reviewedByUserId, resolution);
        }

        public async Task<bool> DismissReportAsync(long reportId, int reviewedByUserId, string? resolution)
        {
            return await _adminReportDAO.DismissReportAsync(reportId, reviewedByUserId, resolution);
        }

        public async Task<bool> MarkInReviewAsync(long reportId, int reviewedByUserId)
        {
            return await _adminReportDAO.MarkInReviewAsync(reportId, reviewedByUserId);
        }

        public async Task<List<Report>> GetReportsOfPostAsync(long postId)
        {
            return await _adminReportDAO.GetReportsOfPostAsync(postId);
        }

        public async Task<List<Report>> GetReportsOfUserAsync(int userId)
        {
            return await _adminReportDAO.GetReportsOfUserAsync(userId);
        }

        public async Task<Dictionary<byte, int>> GetReportReasonStatisticsAsync()
        {
            return await _adminReportDAO.GetReportReasonStatisticsAsync();
        }

        public async Task<int> CountOpenReportsAsync()
        {
            return await _adminReportDAO.CountOpenReportsAsync();
        }

        public async Task<int> CountAllReportsAsync()
        {
            return await _adminReportDAO.CountAllReportsAsync();
        }
    }
}