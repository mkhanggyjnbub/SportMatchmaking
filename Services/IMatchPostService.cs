using BusinessObjects;
using Services.DTOs;

namespace Services.MatchPosts
{
    public interface IMatchPostService
    {
        List<MatchPost> GetPosts(MatchPostSearchDTO? search = null);
        List<MatchPost> GetPostsByCreator(int creatorUserId, MatchPostSearchDTO? search = null);
        MatchPost? GetById(long postId);
        List<Sport> GetSports();
        long Create(CreateMatchPostDTO dto);
        void Update(UpdateMatchPostDTO dto);
        void Cancel(long postId, int currentUserId);
        void UpdateStatus(long postId, int currentUserId, byte status);
        void LeavePost(long postId, int currentUserId);
        void UpdateParticipantStatus(long postId, int participantUserId, int actorUserId, bool isAdmin, byte status);
        void ReportPost(CreatePostReportDTO dto);
        int GetFilledSlots(MatchPost post);
        int GetRemainingSlots(MatchPost post);
    }
}
