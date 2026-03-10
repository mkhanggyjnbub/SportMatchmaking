using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public class AdminReportDAO
    {
        private readonly SportMatchmakingContext _context;

        private const byte REPORT_TARGET_POST = (byte)ReportTargetType.Post;
        private const byte REPORT_TARGET_USER = (byte)ReportTargetType.User;

        private const byte REPORT_STATUS_OPEN = (byte)ReportStatus.Open;
        private const byte REPORT_STATUS_IN_REVIEW = (byte)ReportStatus.InReview;
        private const byte REPORT_STATUS_RESOLVED = (byte)ReportStatus.Resolved;
        private const byte REPORT_STATUS_DISMISSED = (byte)ReportStatus.Dismissed;

        public AdminReportDAO(SportMatchmakingContext context)
        {
            _context = context;
        }

        public async Task<List<Report>> GetReportsAsync(
            byte? status = null,
            byte? targetType = null,
            byte? reasonCode = null,
            int? reporterUserId = null)
        {
            IQueryable<Report> query = _context.Set<Report>().AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            if (targetType.HasValue)
            {
                query = query.Where(x => x.TargetType == targetType.Value);
            }

            if (reasonCode.HasValue)
            {
                query = query.Where(x => x.ReasonCode == reasonCode.Value);
            }

            if (reporterUserId.HasValue)
            {
                query = query.Where(x => x.ReporterUserId == reporterUserId.Value);
            }

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Report?> GetReportByIdAsync(long reportId)
        {
            return await _context.Set<Report>()
                .FirstOrDefaultAsync(x => x.ReportId == reportId);
        }

        public async Task<bool> UpdateReportStatusAsync(
            long reportId,
            byte status,
            int reviewedByUserId,
            string? resolution = null)
        {
            var report = await _context.Set<Report>()
                .FirstOrDefaultAsync(x => x.ReportId == reportId);

            if (report == null)
            {
                return false;
            }

            report.Status = status;
            report.ReviewedByUserId = reviewedByUserId;
            report.ReviewedAt = DateTime.Now;
            report.Resolution = resolution;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResolveReportAsync(
            long reportId,
            int reviewedByUserId,
            string? resolution)
        {
            var report = await _context.Set<Report>()
                .FirstOrDefaultAsync(x => x.ReportId == reportId);

            if (report == null)
            {
                return false;
            }

            report.Status = REPORT_STATUS_RESOLVED;
            report.ReviewedByUserId = reviewedByUserId;
            report.ReviewedAt = DateTime.Now;
            report.Resolution = resolution;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DismissReportAsync(
            long reportId,
            int reviewedByUserId,
            string? resolution)
        {
            var report = await _context.Set<Report>()
                .FirstOrDefaultAsync(x => x.ReportId == reportId);

            if (report == null)
            {
                return false;
            }

            report.Status = REPORT_STATUS_DISMISSED;
            report.ReviewedByUserId = reviewedByUserId;
            report.ReviewedAt = DateTime.Now;
            report.Resolution = resolution;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkInReviewAsync(long reportId, int reviewedByUserId)
        {
            var report = await _context.Set<Report>()
                .FirstOrDefaultAsync(x => x.ReportId == reportId);

            if (report == null)
            {
                return false;
            }

            report.Status = REPORT_STATUS_IN_REVIEW;
            report.ReviewedByUserId = reviewedByUserId;
            report.ReviewedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Report>> GetReportsOfPostAsync(long postId)
        {
            return await _context.Set<Report>()
                .Where(x => x.TargetType == REPORT_TARGET_POST && x.TargetPostId == postId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Report>> GetReportsOfUserAsync(int userId)
        {
            return await _context.Set<Report>()
                .Where(x => x.TargetType == REPORT_TARGET_USER && x.TargetUserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Dictionary<byte, int>> GetReportReasonStatisticsAsync()
        {
            return await _context.Set<Report>()
                .GroupBy(x => x.ReasonCode)
                .Select(g => new
                {
                    ReasonCode = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.ReasonCode, x => x.Count);
        }

        public async Task<int> CountOpenReportsAsync()
        {
            return await _context.Set<Report>()
                .CountAsync(x => x.Status == REPORT_STATUS_OPEN);
        }

        public async Task<int> CountAllReportsAsync()
        {
            return await _context.Set<Report>().CountAsync();
        }
    }
}