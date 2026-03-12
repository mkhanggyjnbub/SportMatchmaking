using BusinessObjects;
using AppUserEntity = BusinessObjects.AppUser;

namespace Services.Admin
{
    public interface IAdminUserService
    {
        Task<List<AppUserEntity>> GetUsersAsync(string? keyword = null, int? roleId = null, bool? isBanned = null);
        Task<AppUserEntity?> GetUserByIdAsync(int userId);
        Task<List<Role>> GetRolesAsync();
        Task<Role?> GetRoleByIdAsync(int roleId);

        Task<(bool Success, string Message)> ToggleBanAsync(int targetUserId, int currentAdminUserId);
        Task<(bool Success, string Message)> UpdateUserRoleAsync(int targetUserId, int newRoleId, int currentAdminUserId);
        Task<(bool Success, string Message)> UpdateUserAsync(AppUserEntity updatedUser);

        Task<int> CountUsersAsync();
        Task<int> CountBannedUsersAsync();
    }
}