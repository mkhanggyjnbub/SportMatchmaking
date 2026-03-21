using BusinessObjects;
using AppUserEntity = BusinessObjects.AppUser;

namespace Repositories.Admin
{
    public interface IAdminUserRepository
    {
        Task<List<AppUserEntity>> GetUsersAsync(string? keyword = null, int? roleId = null, bool? isBanned = null);
        Task<AppUserEntity?> GetUserByIdAsync(int userId);
        Task<List<Role>> GetRolesAsync();
        Task<Role?> GetRoleByIdAsync(int roleId);
        Task<bool> ToggleBanAsync(int userId);
        Task<bool> UpdateUserAsync(AppUserEntity updatedUser);
        Task<bool> ExistsByEmailAsync(string email, int? excludeUserId = null);
        Task<bool> ExistsByUserNameAsync(string userName, int? excludeUserId = null);
        Task<int> CountUsersAsync();
        Task<int> CountBannedUsersAsync();
    }
}