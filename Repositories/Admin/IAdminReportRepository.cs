using BusinessObjects;

namespace Repositories.Admin
{
    public interface IAdminReportRepository
    {
        Task<List<Report>> GetReportsAsync(
            byte? status = null,
            byte? targetType = null,
            byte? reasonCode = null,
            int? reporterUserId = null);

        Task<Report?> GetReportByIdAsync(long reportId);
        Task<bool> UpdateReportStatusAsync(long reportId, byte status, int reviewedByUserId, string? resolution = null);
        Task<bool> ResolveReportAsync(long reportId, int reviewedByUserId, string? resolution);
        Task<bool> DismissReportAsync(long reportId, int reviewedByUserId, string? resolution);
        Task<bool> MarkInReviewAsync(long reportId, int reviewedByUserId);
        Task<List<Report>> GetReportsOfPostAsync(long postId);
        Task<List<Report>> GetReportsOfUserAsync(int userId);
        Task<Dictionary<byte, int>> GetReportReasonStatisticsAsync();
        Task<int> CountOpenReportsAsync();
        Task<int> CountAllReportsAsync();
    }
}