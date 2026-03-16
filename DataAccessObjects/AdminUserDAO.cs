using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public class AdminUserDAO
    {
        private readonly SportMatchmakingContext _context;

        public AdminUserDAO(SportMatchmakingContext context)
        {
            _context = context;
        }

        public async Task<List<AppUser>> GetUsersAsync(
            string? keyword = null,
            int? roleId = null,
            bool? isBanned = null)
        {
            IQueryable<AppUser> query = _context.Set<AppUser>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>
                    x.Email.Contains(keyword) ||
                    x.UserName.Contains(keyword) ||
                    (x.DisplayName != null && x.DisplayName.Contains(keyword)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.Contains(keyword)) ||
                    (x.City != null && x.City.Contains(keyword)) ||
                    (x.District != null && x.District.Contains(keyword)));
            }

            if (roleId.HasValue)
            {
                query = query.Where(x => x.RoleId == roleId.Value);
            }

            if (isBanned.HasValue)
            {
                query = query.Where(x => x.IsBanned == isBanned.Value);
            }

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<AppUser?> GetUserByIdAsync(int userId)
        {
            return await _context.Set<AppUser>()
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<List<Role>> GetRolesAsync()
        {
            return await _context.Set<Role>()
                .OrderBy(x => x.RoleId)
                .ToListAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _context.Set<Role>()
                .FirstOrDefaultAsync(x => x.RoleId == roleId);
        }

        public async Task<bool> ToggleBanAsync(int userId)
        {
            var user = await _context.Set<AppUser>()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (user == null)
            {
                return false;
            }

            user.IsBanned = !user.IsBanned;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserAsync(AppUser updatedUser)
        {
            var existingUser = await _context.Set<AppUser>()
                .FirstOrDefaultAsync(x => x.UserId == updatedUser.UserId);

            if (existingUser == null)
            {
                return false;
            }

            existingUser.DisplayName = updatedUser.DisplayName;
            existingUser.PhoneNumber = updatedUser.PhoneNumber;
            existingUser.AvatarUrl = updatedUser.AvatarUrl;
            existingUser.Bio = updatedUser.Bio;
            existingUser.City = updatedUser.City;
            existingUser.District = updatedUser.District;
            existingUser.SkillLevel = updatedUser.SkillLevel;
            existingUser.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByEmailAsync(string email, int? excludeUserId = null)
        {
            IQueryable<AppUser> query = _context.Set<AppUser>()
                .Where(x => x.Email == email);

            if (excludeUserId.HasValue)
            {
                query = query.Where(x => x.UserId != excludeUserId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> ExistsByUserNameAsync(string userName, int? excludeUserId = null)
        {
            IQueryable<AppUser> query = _context.Set<AppUser>()
                .Where(x => x.UserName == userName);

            if (excludeUserId.HasValue)
            {
                query = query.Where(x => x.UserId != excludeUserId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> CountUsersAsync()
        {
            return await _context.Set<AppUser>().CountAsync();
        }

        public async Task<int> CountBannedUsersAsync()
        {
            return await _context.Set<AppUser>()
                .CountAsync(x => x.IsBanned);
        }
    }
}