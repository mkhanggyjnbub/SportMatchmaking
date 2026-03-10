using BusinessObjects;

namespace Repositories.Interfaces
{
    public interface IJoinRequestRepository
    {
        JoinRequest? GetById(long requestId);
        JoinRequest? GetPendingRequest(long postId, int requesterUserId);
        List<JoinRequest> GetRequestsByPostId(long postId);
        List<JoinRequest> GetRequestsByRequesterId(int requesterUserId);
        void Add(JoinRequest request);
        void Update(JoinRequest request);
        void Save();
    }
}