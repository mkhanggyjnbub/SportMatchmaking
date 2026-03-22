using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessObjects
{
    public class JoinRequestDAO
    {
        private readonly SportMatchmakingContext _context;

        public JoinRequestDAO(SportMatchmakingContext context)
        {
            _context = context;
        }

        public MatchPost? GetPostById(long postId)
        {
            return _context.MatchPosts
                .FirstOrDefault(x => x.PostId == postId);
        }

        public BusinessObjects.JoinRequest? GetPendingRequest(long postId, int requesterUserId)
        {
            return _context.JoinRequests
                .FirstOrDefault(x =>
                    x.PostId == postId &&
                    x.RequesterUserId == requesterUserId &&
                    x.Status == 1); // 1 = Pending
        }

        public int GetConfirmedParticipantSlots(long postId)
        {
            return _context.PostParticipants
                .Where(x =>
                    x.PostId == postId &&
                    x.Role == 2 &&     // 2 = Participant
                    x.Status == 1)     // 1 = Confirmed
                .Sum(x => (int?)x.PartySize) ?? 0;
        }


        public List<BusinessObjects.JoinRequest> GetRequestsByPostId(long postId)
        {
            return _context.JoinRequests
                .Include(x => x.RequesterUser)
                .Where(x => x.PostId == postId)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        public void Add(BusinessObjects.JoinRequest entity)
        {
            _context.JoinRequests.Add(entity);
        }
        public List<BusinessObjects.JoinRequest> GetRequestsByRequesterUserId(int requesterUserId)
        {
            return _context.JoinRequests
                .Include(x => x.Post)
                .Where(x => x.RequesterUserId == requesterUserId)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        public BusinessObjects.JoinRequest? GetById(long requestId)
        {
            return _context.JoinRequests
                .Include(x => x.Post)
                .FirstOrDefault(x => x.RequestId == requestId);
        }

        public void Update(BusinessObjects.JoinRequest entity)
        {
            _context.JoinRequests.Update(entity);
        }

        public void UpdatePost(MatchPost post)
        {
            _context.MatchPosts.Update(post);
        }

        public AppUser? GetUserById(int userId)
        {
            return _context.AppUsers
                .FirstOrDefault(x => x.UserId == userId);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
        public BusinessObjects.JoinRequest? GetByPostAndRequester(long postId, int requesterUserId)
        {
            return _context.JoinRequests
                .FirstOrDefault(x => x.PostId == postId && x.RequesterUserId == requesterUserId);
        }
    }
}
