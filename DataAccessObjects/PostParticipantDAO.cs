using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public void Add(PostParticipant entity)
        {
            _context.PostParticipants.Add(entity);
        }

        public void Update(PostParticipant entity)
        {
            _context.PostParticipants.Update(entity);
        }

        public async Task<List<PostParticipant>> GetByPostIdAsync(long postId)
        {
            return await _context.PostParticipants
                .Include(x => x.User)
                .Where(x => x.PostId == postId)
                .OrderBy(x => x.JoinedAt)
                .ToListAsync();
        }

        public async Task<int> GetConfirmedParticipantSlotsAsync(long postId)
        {
            return await _context.PostParticipants
                .Where(x =>
                    x.PostId == postId &&
                    x.Role == (byte)ParticipantRole.Participant &&
                    x.Status == (byte)ParticipantStatus.Confirmed)
                .SumAsync(x => (int?)x.PartySize) ?? 0;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
