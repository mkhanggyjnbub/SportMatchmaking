using BusinessObjects;

namespace Services.PostParticipant
{
    public interface IPostParticipantService
    {
        Task<List<BusinessObjects.PostParticipant>> GetParticipantsAsync(long postId);
        Task<(bool Success, string Message)> LeavePostAsync(long postId, int userId);
        Task<(bool Success, string Message)> MarkNoShowAsync(long postId, int targetUserId, int actorUserId, bool actorIsAdmin);
    }
}
