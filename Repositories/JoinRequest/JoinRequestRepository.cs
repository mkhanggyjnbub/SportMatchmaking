using BusinessObjects;
using DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.JoinRequest
{
    public class JoinRequestRepository: IJoinRequestRepository
    {
        private readonly JoinRequestDAO _joinRequestDAO;

        public JoinRequestRepository(JoinRequestDAO joinRequestDAO)
        {
            _joinRequestDAO = joinRequestDAO;
        }

        public MatchPost? GetPostById(long postId)
        {
            return _joinRequestDAO.GetPostById(postId);
        }


        public BusinessObjects.JoinRequest? GetPendingRequest(long postId, int requesterUserId)
        {
            return _joinRequestDAO.GetPendingRequest(postId, requesterUserId);
        }

        public int GetConfirmedParticipantSlots(long postId)
        {
            return _joinRequestDAO.GetConfirmedParticipantSlots(postId);
        }

        public List<BusinessObjects.JoinRequest> GetRequestsByPostId(long postId)
        {
            return _joinRequestDAO.GetRequestsByPostId(postId);
        }

        public List<BusinessObjects.JoinRequest> GetRequestsByRequesterUserId(int requesterUserId)
        {
            return _joinRequestDAO.GetRequestsByRequesterUserId(requesterUserId);
        }

        public void Add(BusinessObjects.JoinRequest entity)
        {
            _joinRequestDAO.Add(entity);
        }

        public void Update(BusinessObjects.JoinRequest entity)
        {
            _joinRequestDAO.Update(entity);
        }

        public void UpdatePost(MatchPost post)
        {
            _joinRequestDAO.UpdatePost(post);
        }
        public void Save()
        {
            _joinRequestDAO.Save();
        }

        public BusinessObjects.JoinRequest? GetById(long requestId)
        {
            return _joinRequestDAO.GetById(requestId);
        }

        BusinessObjects.AppUser? IJoinRequestRepository.GetUserById(int userId)
        {
            return _joinRequestDAO.GetUserById(userId);
        }
    }
}
