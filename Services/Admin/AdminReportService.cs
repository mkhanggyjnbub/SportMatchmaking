using BusinessObjects;
using Repositories.Admin;
using Repositories.MatchPosts;
using Services.Notifications;

namespace Services.Admin
{
    public class AdminReportService : IAdminReportService
    {
        private readonly IAdminReportRepository _adminReportRepository;
        private readonly IMatchPostRepository _matchPostRepository;
        private readonly INotificationService _notificationService;

        private const byte REPORT_STATUS_OPEN = 1;
        private const byte REPORT_STATUS_IN_REVIEW = 2;
        private const byte REPORT_STATUS_RESOLVED = 3;
        private const byte REPORT_STATUS_DISMISSED = 4;

        public AdminReportService(
            IAdminReportRepository adminReportRepository,
            IMatchPostRepository matchPostRepository,
            INotificationService notificationService)
        {
            _adminReportRepository = adminReportRepository;
            _matchPostRepository = matchPostRepository;
            _notificationService = notificationService;
        }

        public async Task<List<Report>> GetReportsAsync(
            byte? status = null,
            byte? targetType = null,
            byte? reasonCode = null,
            int? reporterUserId = null)
        {
            return await _adminReportRepository.GetReportsAsync(status, targetType, reasonCode, reporterUserId);
        }

        public async Task<Report?> GetReportByIdAsync(long reportId)
        {
            return await _adminReportRepository.GetReportByIdAsync(reportId);
        }

        public async Task<(bool Success, string Message)> UpdateReportStatusAsync(
            long reportId,
            byte status,
            int reviewedByUserId,
            string? resolution = null)
        {
            if (reportId <= 0 || reviewedByUserId <= 0)
            {
                return (false, "Dữ liệu không hợp lệ.");
            }

            if (status != REPORT_STATUS_OPEN &&
                status != REPORT_STATUS_IN_REVIEW &&
                status != REPORT_STATUS_RESOLVED &&
                status != REPORT_STATUS_DISMISSED)
            {
                return (false, "Trạng thái report không hợp lệ.");
            }

            var report = await _adminReportRepository.GetReportByIdAsync(reportId);
            if (report == null)
            {
                return (false, "Không tìm thấy report.");
            }

            bool result = await _adminReportRepository.UpdateReportStatusAsync(
                reportId,
                status,
                reviewedByUserId,
                resolution);

            return result
                ? (true, "Cập nhật trạng thái report thành công.")
                : (false, "Cập nhật trạng thái report thất bại.");
        }

        public async Task<(bool Success, string Message)> ResolveReportAsync(
            long reportId,
            int reviewedByUserId,
            string? resolution)
        {
            if (reportId <= 0 || reviewedByUserId <= 0)
            {
                return (false, "Dữ liệu không hợp lệ.");
            }

            var report = await _adminReportRepository.GetReportByIdAsync(reportId);
            if (report == null)
            {
                return (false, "Không tìm thấy report.");
            }

            bool result = await _adminReportRepository.ResolveReportAsync(reportId, reviewedByUserId, resolution);
            if (result)
            {
                await CancelPostWhenResolvedReportsReachThresholdAsync(report);
                _notificationService.NotifyReportResolved(report);
            }

            return result
                ? (true, "Xử lý report thành công.")
                : (false, "Xử lý report thất bại.");
        }

        public async Task<(bool Success, string Message)> DismissReportAsync(
            long reportId,
            int reviewedByUserId,
            string? resolution)
        {
            if (reportId <= 0 || reviewedByUserId <= 0)
            {
                return (false, "Dữ liệu không hợp lệ.");
            }

            var report = await _adminReportRepository.GetReportByIdAsync(reportId);
            if (report == null)
            {
                return (false, "Không tìm thấy report.");
            }

            bool result = await _adminReportRepository.DismissReportAsync(reportId, reviewedByUserId, resolution);
            if (result)
            {
                _notificationService.NotifyReportDismissed(report);
            }

            return result
                ? (true, "Từ chối report thành công.")
                : (false, "Từ chối report thất bại.");
        }

        public async Task<(bool Success, string Message)> MarkInReviewAsync(long reportId, int reviewedByUserId)
        {
            if (reportId <= 0 || reviewedByUserId <= 0)
            {
                return (false, "Dữ liệu không hợp lệ.");
            }

            var report = await _adminReportRepository.GetReportByIdAsync(reportId);
            if (report == null)
            {
                return (false, "Không tìm thấy report.");
            }

            bool result = await _adminReportRepository.MarkInReviewAsync(reportId, reviewedByUserId);

            return result
                ? (true, "Đã chuyển report sang trạng thái đang xử lý.")
                : (false, "Cập nhật trạng thái thất bại.");
        }

        public async Task<List<Report>> GetReportsOfPostAsync(long postId)
        {
            return await _adminReportRepository.GetReportsOfPostAsync(postId);
        }

        public async Task<List<Report>> GetReportsOfUserAsync(int userId)
        {
            return await _adminReportRepository.GetReportsOfUserAsync(userId);
        }

        public async Task<Dictionary<byte, int>> GetReportReasonStatisticsAsync()
        {
            return await _adminReportRepository.GetReportReasonStatisticsAsync();
        }

        public async Task<int> CountOpenReportsAsync()
        {
            return await _adminReportRepository.CountOpenReportsAsync();
        }

        public async Task<int> CountAllReportsAsync()
        {
            return await _adminReportRepository.CountAllReportsAsync();
        }

        private async Task CancelPostWhenResolvedReportsReachThresholdAsync(Report report)
        {
            if (report.TargetType != (byte)ReportTargetType.Post || !report.TargetPostId.HasValue)
            {
                return;
            }

            var post = _matchPostRepository.GetById(report.TargetPostId.Value);
            if (post == null || post.Status == (byte)PostStatus.Cancelled)
            {
                return;
            }

            var reportsOfPost = await _adminReportRepository.GetReportsOfPostAsync(report.TargetPostId.Value);
            var requiredResolvedReports = post.SlotsNeeded == 1 ? 1 : 3;
            if (reportsOfPost.Count != requiredResolvedReports)
            {
                return;
            }

            post.Status = (byte)PostStatus.Cancelled;
            post.UpdatedAt = DateTime.Now;
            _matchPostRepository.Update(post);
        }
    }
}
