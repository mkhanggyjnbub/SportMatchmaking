using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public class AdminSportDAO
    {
        private readonly SportMatchmakingContext _context;

        public AdminSportDAO(SportMatchmakingContext context)
        {
            _context = context;
        }

        // =========================
        // SPORT
        // =========================

        public async Task<List<Sport>> GetSportsAsync(string? keyword = null)
        {
            IQueryable<Sport> query = _context.Set<Sport>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(x => x.Name.Contains(keyword));
            }

            return await query
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<Sport?> GetSportByIdAsync(int sportId)
        {
            return await _context.Set<Sport>()
                .FirstOrDefaultAsync(x => x.SportId == sportId);
        }

        public async Task<Sport> AddSportAsync(Sport sport)
        {
            sport.CreatedAt = DateTime.Now;

            _context.Set<Sport>().Add(sport);
            await _context.SaveChangesAsync();

            return sport;
        }

        public async Task<bool> UpdateSportAsync(Sport updatedSport)
        {
            var existingSport = await _context.Set<Sport>()
                .FirstOrDefaultAsync(x => x.SportId == updatedSport.SportId);

            if (existingSport == null)
            {
                return false;
            }

            existingSport.Name = updatedSport.Name;
            existingSport.TeamMin = updatedSport.TeamMin;
            existingSport.TeamMax = updatedSport.TeamMax;
            existingSport.ImageId = updatedSport.ImageId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSportAsync(int sportId)
        {
            var sport = await _context.Set<Sport>()
                .FirstOrDefaultAsync(x => x.SportId == sportId);

            if (sport == null)
            {
                return false;
            }

            _context.Set<Sport>().Remove(sport);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsSportInUseAsync(int sportId)
        {
            return await _context.Set<MatchPost>()
                .AnyAsync(x => x.SportId == sportId);
        }

        public async Task<bool> SportNameExistsAsync(string sportName, int? excludeSportId = null)
        {
            IQueryable<Sport> query = _context.Set<Sport>()
                .Where(x => x.Name == sportName);

            if (excludeSportId.HasValue)
            {
                query = query.Where(x => x.SportId != excludeSportId.Value);
            }

            return await query.AnyAsync();
        }

        // =========================
        // SPORT IMAGE
        // =========================

        public async Task<List<SportImage>> GetSportImagesAsync()
        {
            return await _context.Set<SportImage>()
                .OrderByDescending(x => x.ImageId)
                .ToListAsync();
        }

        public async Task<SportImage?> GetSportImageByIdAsync(int imageId)
        {
            return await _context.Set<SportImage>()
                .FirstOrDefaultAsync(x => x.ImageId == imageId);
        }

        public async Task<SportImage> AddSportImageAsync(SportImage sportImage)
        {
            _context.Set<SportImage>().Add(sportImage);
            await _context.SaveChangesAsync();

            return sportImage;
        }

        public async Task<bool> UpdateSportImageAsync(SportImage updatedImage)
        {
            var existingImage = await _context.Set<SportImage>()
                .FirstOrDefaultAsync(x => x.ImageId == updatedImage.ImageId);

            if (existingImage == null)
            {
                return false;
            }

            existingImage.ImageUrl = updatedImage.ImageUrl;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSportImageAsync(int imageId)
        {
            bool isInUse = await _context.Set<Sport>()
                .AnyAsync(x => x.ImageId == imageId);

            if (isInUse)
            {
                return false;
            }

            var image = await _context.Set<SportImage>()
                .FirstOrDefaultAsync(x => x.ImageId == imageId);

            if (image == null)
            {
                return false;
            }

            _context.Set<SportImage>().Remove(image);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}