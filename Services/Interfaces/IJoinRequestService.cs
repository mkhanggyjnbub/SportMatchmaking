using Services.Models.JoinRequest;

namespace Services.Interfaces
{
    public interface IJoinRequestService
    {
        (bool Success, string Message) CreateRequest(int requesterUserId, CreateJoinRequestModel model);
        List<JoinRequestListItemModel> GetRequestsByPostId(long postId, int currentUserId);
        List<JoinRequestListItemModel> GetMyRequests(int requesterUserId);
        (bool Success, string Message) CancelRequest(long requestId, int requesterUserId);
        (bool Success, string Message) ReviewRequest(int ownerUserId, ReviewJoinRequestModel model);
    }
}