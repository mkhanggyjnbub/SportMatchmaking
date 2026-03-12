using Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppUserProfile = BusinessObjects.AppUser;

namespace Services.AppUser
{
    public interface IProfileService
    {
        AppUserProfile? GetById(int userId);
        void UpdateProfile(UpdateProfileDTO dto);
        void ChangePassword(int userId, string oldPassword, string newPassword);
    }
}
