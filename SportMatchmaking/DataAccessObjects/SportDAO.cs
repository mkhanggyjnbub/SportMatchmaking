using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public class SportDAO
    {
        private readonly SportMatchmakingContext _context;

        public SportDAO(SportMatchmakingContext context)
        {
            _context = context;
        }

        public async Task<List<Sport>> GetSportsAsync()
        {
            return await _context.Set<Sport>()
                .Include(x => x.Image)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<Sport?> GetSportByIdAsync(int sportId)
        {
            return await _context.Set<Sport>()
                .Include(x => x.Image)
                .FirstOrDefaultAsync(x => x.SportId == sportId);
        }
    }
}
