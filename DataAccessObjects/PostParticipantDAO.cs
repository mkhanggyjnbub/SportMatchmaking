using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessObjects
{
    public class PostParticipantDAO
    {
        private readonly SportMatchmakingContext _context;

        public PostParticipantDAO(SportMatchmakingContext context)
        {
            _context = context;
        }

        public PostParticipant? GetByPostAndUser(long postId, int userId)
        {
            return _context.PostParticipants
                .FirstOrDefault(x => x.PostId == postId && x.UserId == userId);
        }

        public IQueryable<PostParticipant> GetQueryable()
        {
            return _context.PostParticipants.AsQueryable();
        }

        public void Add(PostParticipant entity)
        {
            _context.PostParticipants.Add(entity);
        }

        public void Update(PostParticipant entity)
        {
            _context.PostParticipants.Update(entity);
        }
    }
}
