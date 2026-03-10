using BusinessObjects;

namespace Services.Admin
{
    public interface IAdminReportService
    {
        Task<List<Report>> GetReportsAsync(
            byte? status = null,
            byte? targetType = null,
            byte? reasonCode = null,
            int? reporterUserId = null);

        Task<Report?> GetReportByIdAsync(long reportId);

        Task<(bool Success, string Message)> UpdateReportStatusAsync(
            long reportId,
            byte status,
            int reviewedByUserId,
            string? resolution = null);

        Task<(bool Success, string Message)> ResolveReportAsync(
            long reportId,
            int reviewedByUserId,
            string? resolution);

        Task<(bool Success, string Message)> DismissReportAsync(
            long reportId,
            int reviewedByUserId,
            string? resolution);

        Task<(bool Success, string Message)> MarkInReviewAsync(long reportId, int reviewedByUserId);

        Task<List<Report>> GetReportsOfPostAsync(long postId);
        Task<List<Report>> GetReportsOfUserAsync(int userId);
        Task<Dictionary<byte, int>> GetReportReasonStatisticsAsync();

        Task<int> CountOpenReportsAsync();
        Task<int> CountAllReportsAsync();
    }
}