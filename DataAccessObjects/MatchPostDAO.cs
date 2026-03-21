using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public class MatchPostDAO
    {
        private readonly SportMatchmakingContext _context;

        public MatchPostDAO(SportMatchmakingContext context)
        {
            _context = context;
        }

        public MatchPost? GetById(long postId)
        {
            return _context.MatchPosts
                .Include(x => x.Sport)
                    .ThenInclude(s => s.Image)
                .Include(x => x.CreatorUser)
                .Include(x => x.PostParticipants)
                    .ThenInclude(pp => pp.User)
                .Include(x => x.JoinRequests)
                .Include(x => x.Reports)
                .FirstOrDefault(x => x.PostId == postId);
        }

        public IQueryable<MatchPost> GetQueryable()
        {
            return _context.MatchPosts
                .Include(x => x.Sport)
                    .ThenInclude(s => s.Image)
                .Include(x => x.CreatorUser)
                .Include(x => x.PostParticipants)
                    .ThenInclude(pp => pp.User)
                .Include(x => x.Reports);
        }

        public List<Sport> GetSports()
        {
            return _context.Sports
                .Include(x => x.Image)
                .OrderBy(x => x.Name)
                .ToList();
        }

        public Sport? GetSportById(int sportId)
        {
            return _context.Sports
                .Include(x => x.Image)
                .FirstOrDefault(x => x.SportId == sportId);
        }

        public AppUser? GetUserById(int userId)
        {
            return _context.AppUsers
                .FirstOrDefault(x => x.UserId == userId);
        }

        public Report? GetActivePostReportByReporter(long postId, int reporterUserId)
        {
            return _context.Reports
                .FirstOrDefault(x =>
                    x.TargetType == (byte)ReportTargetType.Post &&
                    x.TargetPostId == postId &&
                    x.ReporterUserId == reporterUserId &&
                    (x.Status == (byte)ReportStatus.Open || x.Status == (byte)ReportStatus.InReview));
        }

        public void AddWithCreatorParticipant(MatchPost post, PostParticipant creatorParticipant)
        {
            _context.MatchPosts.Add(post);
            _context.PostParticipants.Add(creatorParticipant);
            _context.SaveChanges();
        }

        public void Add(MatchPost post)
        {
            _context.MatchPosts.Add(post);
            _context.SaveChanges();
        }

        public void Update(MatchPost post)
        {
            _context.MatchPosts.Update(post);
            _context.SaveChanges();
        }

        public void AddReport(Report report)
        {
            _context.Reports.Add(report);
            _context.SaveChanges();
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
