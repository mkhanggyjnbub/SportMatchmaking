using System;
using System.Linq;
using BusinessObjects;
using Repositories.Admin;
using AppUserEntity = BusinessObjects.AppUser;

namespace Services.Admin
{
    public class AdminUserService : IAdminUserService
    {
        private readonly IAdminUserRepository _adminUserRepository;

        public AdminUserService(IAdminUserRepository adminUserRepository)
        {
            _adminUserRepository = adminUserRepository;
        }

        public async Task<List<AppUserEntity>> GetUsersAsync(string? keyword = null, int? roleId = null, bool? isBanned = null)
        {
            return await _adminUserRepository.GetUsersAsync(keyword, roleId, isBanned);
        }

        public async Task<AppUserEntity?> GetUserByIdAsync(int userId)
        {
            return await _adminUserRepository.GetUserByIdAsync(userId);
        }

        public async Task<List<Role>> GetRolesAsync()
        {
            return await _adminUserRepository.GetRolesAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _adminUserRepository.GetRoleByIdAsync(roleId);
        }

        public async Task<(bool Success, string Message)> ToggleBanAsync(int targetUserId, int currentAdminUserId)
        {
            if (targetUserId <= 0)
            {
                return (false, "User không hợp lệ.");
            }

            if (targetUserId == currentAdminUserId)
            {
                return (false, "Bạn không thể tự khóa tài khoản của chính mình.");
            }

            var user = await _adminUserRepository.GetUserByIdAsync(targetUserId);
            if (user == null)
            {
                return (false, "Không tìm thấy user.");
            }

            // Không cho phép khóa tài khoản có role Admin.
            var roles = await _adminUserRepository.GetRolesAsync();
            var adminRole = roles.FirstOrDefault(r =>
                string.Equals(r.Name, "Admin", StringComparison.OrdinalIgnoreCase));

            // ToggleBanAsync sẽ đổi trạng thái IsBanned. Chỉ chặn trường hợp "khóa" (tức đang chưa bị khóa).
            if (adminRole != null && user.RoleId == adminRole.RoleId && !user.IsBanned)
            {
                return (false, "Không thể khóa tài khoản admin.");
            }

            bool result = await _adminUserRepository.ToggleBanAsync(targetUserId);

            return result
                ? (true, user.IsBanned ? "Mở khóa tài khoản thành công." : "Khóa tài khoản thành công.")
                : (false, "Cập nhật trạng thái khóa thất bại.");
        }

        public async Task<(bool Success, string Message)> UpdateUserAsync(AppUserEntity updatedUser)
        {
            if (updatedUser == null || updatedUser.UserId <= 0)
            {
                return (false, "Dữ liệu user không hợp lệ.");
            }

            var existingUser = await _adminUserRepository.GetUserByIdAsync(updatedUser.UserId);
            if (existingUser == null)
            {
                return (false, "Không tìm thấy user.");
            }

            if (!string.IsNullOrWhiteSpace(updatedUser.Email))
            {
                bool emailExists = await _adminUserRepository.ExistsByEmailAsync(updatedUser.Email, updatedUser.UserId);
                if (emailExists)
                {
                    return (false, "Email đã tồn tại.");
                }
            }

            if (!string.IsNullOrWhiteSpace(updatedUser.UserName))
            {
                bool userNameExists = await _adminUserRepository.ExistsByUserNameAsync(updatedUser.UserName, updatedUser.UserId);
                if (userNameExists)
                {
                    return (false, "UserName đã tồn tại.");
                }
            }

            bool result = await _adminUserRepository.UpdateUserAsync(updatedUser);

            return result
                ? (true, "Cập nhật user thành công.")
                : (false, "Cập nhật user thất bại.");
        }

        public async Task<int> CountUsersAsync()
        {
            return await _adminUserRepository.CountUsersAsync();
        }

        public async Task<int> CountBannedUsersAsync()
        {
            return await _adminUserRepository.CountBannedUsersAsync();
        }
    }
}