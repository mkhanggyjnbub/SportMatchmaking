using BusinessObjects;
using DataAccessObjects;

namespace Repositories.MatchPosts
{
    public class MatchPostRepository : IMatchPostRepository
    {
        private readonly MatchPostDAO _matchPostDao;

        public MatchPostRepository(MatchPostDAO matchPostDao)
        {
            _matchPostDao = matchPostDao;
        }

        public MatchPost? GetById(long postId)
        {
            return _matchPostDao.GetById(postId);
        }

        public IQueryable<MatchPost> GetQueryable()
        {
            return _matchPostDao.GetQueryable();
        }

        public List<Sport> GetSports()
        {
            return _matchPostDao.GetSports();
        }

        public Sport? GetSportById(int sportId)
        {
            return _matchPostDao.GetSportById(sportId);
        }

        public BusinessObjects.AppUser? GetUserById(int userId)
        {
            return _matchPostDao.GetUserById(userId);
        }

        public Report? GetActivePostReportByReporter(long postId, int reporterUserId)
        {
            return _matchPostDao.GetActivePostReportByReporter(postId, reporterUserId);
        }

        public void AddWithCreatorParticipant(MatchPost post, BusinessObjects.PostParticipant creatorParticipant)
        {
            _matchPostDao.AddWithCreatorParticipant(post, creatorParticipant);
        }

        public void Add(MatchPost post)
        {
            _matchPostDao.Add(post);
        }

        public void Update(MatchPost post)
        {
            _matchPostDao.Update(post);
        }

        public void AddReport(Report report)
        {
            _matchPostDao.AddReport(report);
        }

        public void Save()
        {
            _matchPostDao.Save();
        }
    }
}
