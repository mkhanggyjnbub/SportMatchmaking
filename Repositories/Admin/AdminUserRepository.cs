using BusinessObjects;
using DataAccessObjects;
using AppUserEntity = BusinessObjects.AppUser;

namespace Repositories.Admin
{
    public class AdminUserRepository : IAdminUserRepository
    {
        private readonly AdminUserDAO _adminUserDAO;

        public AdminUserRepository(AdminUserDAO adminUserDAO)
        {
            _adminUserDAO = adminUserDAO;
        }

        public async Task<List<AppUserEntity>> GetUsersAsync(string? keyword = null, int? roleId = null, bool? isBanned = null)
        {
            return await _adminUserDAO.GetUsersAsync(keyword, roleId, isBanned);
        }

        public async Task<AppUserEntity?> GetUserByIdAsync(int userId)
        {
            return await _adminUserDAO.GetUserByIdAsync(userId);
        }

        public async Task<List<Role>> GetRolesAsync()
        {
            return await _adminUserDAO.GetRolesAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _adminUserDAO.GetRoleByIdAsync(roleId);
        }

        public async Task<bool> ToggleBanAsync(int userId)
        {
            return await _adminUserDAO.ToggleBanAsync(userId);
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, int roleId)
        {
            return await _adminUserDAO.UpdateUserRoleAsync(userId, roleId);
        }

        public async Task<bool> UpdateUserAsync(AppUserEntity updatedUser)
        {
            return await _adminUserDAO.UpdateUserAsync(updatedUser);
        }

        public async Task<bool> ExistsByEmailAsync(string email, int? excludeUserId = null)
        {
            return await _adminUserDAO.ExistsByEmailAsync(email, excludeUserId);
        }

        public async Task<bool> ExistsByUserNameAsync(string userName, int? excludeUserId = null)
        {
            return await _adminUserDAO.ExistsByUserNameAsync(userName, excludeUserId);
        }

        public async Task<int> CountUsersAsync()
        {
            return await _adminUserDAO.CountUsersAsync();
        }

        public async Task<int> CountBannedUsersAsync()
        {
            return await _adminUserDAO.CountBannedUsersAsync();
        }
    }
}