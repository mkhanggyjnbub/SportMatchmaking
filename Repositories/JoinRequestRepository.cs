using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories
{
    public class JoinRequestRepository : IJoinRequestRepository
    {
        private readonly SportMatchmakingContext _context;

        public JoinRequestRepository(SportMatchmakingContext context)
        {
            _context = context;
        }

        public JoinRequest? GetById(long requestId)
        {
            return _context.JoinRequests
                .Include(x => x.RequesterUser)
                .Include(x => x.Post)
                .FirstOrDefault(x => x.RequestId == requestId);
        }

        public JoinRequest? GetPendingRequest(long postId, int requesterUserId)
        {
            return _context.JoinRequests.FirstOrDefault(x =>
                x.PostId == postId &&
                x.RequesterUserId == requesterUserId &&
                x.Status == 1);
        }

        public List<JoinRequest> GetRequestsByPostId(long postId)
        {
            return _context.JoinRequests
                .Include(x => x.RequesterUser)
                .Where(x => x.PostId == postId)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        public List<JoinRequest> GetRequestsByRequesterId(int requesterUserId)
        {
            return _context.JoinRequests
                .Include(x => x.Post)
                .Include(x => x.RequesterUser)
                .Where(x => x.RequesterUserId == requesterUserId)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        public void Add(JoinRequest request)
        {
            _context.JoinRequests.Add(request);
        }

        public void Update(JoinRequest request)
        {
            _context.JoinRequests.Update(request);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}