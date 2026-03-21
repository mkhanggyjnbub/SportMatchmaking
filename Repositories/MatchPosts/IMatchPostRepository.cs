using BusinessObjects;

namespace Repositories.MatchPosts
{
    public interface IMatchPostRepository
    {
        MatchPost? GetById(long postId);
        IQueryable<MatchPost> GetQueryable();
        List<Sport> GetSports();
        Sport? GetSportById(int sportId);
        BusinessObjects.AppUser? GetUserById(int userId);
        Report? GetActivePostReportByReporter(long postId, int reporterUserId);
        void AddWithCreatorParticipant(MatchPost post, BusinessObjects.PostParticipant creatorParticipant);
        void Add(MatchPost post);
        void Update(MatchPost post);
        void AddReport(Report report);
        void Save();
    }
}
