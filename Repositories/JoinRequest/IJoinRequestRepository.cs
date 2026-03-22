using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppUsers = BusinessObjects.AppUser;

namespace Repositories.JoinRequest
{
    public interface IJoinRequestRepository
    {
        MatchPost? GetPostById(long postId);
        AppUsers? GetUserById(int userId);
        BusinessObjects.JoinRequest? GetById(long requestId);
        BusinessObjects.JoinRequest? GetPendingRequest(long postId, int requesterUserId);
        int GetConfirmedParticipantSlots(long postId);
        List<BusinessObjects.JoinRequest> GetRequestsByPostId(long postId);
        List<BusinessObjects.JoinRequest> GetRequestsByRequesterUserId(int requesterUserId);
        BusinessObjects.JoinRequest? GetByPostAndRequester(long postId, int requesterUserId);
        void Add(BusinessObjects.JoinRequest entity);
        void Update(BusinessObjects.JoinRequest entity);
        void UpdatePost(MatchPost post);
        void Save();
    }
}
