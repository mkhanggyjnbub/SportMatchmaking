using BusinessObjects;

namespace Repositories.Interfaces
{
    public interface IPostParticipantRepository
    {
        List<PostParticipant> GetConfirmedParticipantsByPostId(long postId);
        PostParticipant? GetByPostAndUser(long postId, int userId);
        void Add(PostParticipant participant);
        void Update(PostParticipant participant);
        void Save();
    }
}