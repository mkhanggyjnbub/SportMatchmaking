using Microsoft.AspNetCore.Identity;
using Repositories.AppUser;
using Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Services.AppUser
{
    public class ProfileService : IProfileService
    {
        private readonly IAppUserRepository _userRepository;
        private readonly PasswordHasher<BusinessObjects.AppUser> _passwordHasher;

        public ProfileService(IAppUserRepository userRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = new PasswordHasher<BusinessObjects.AppUser>();
        }

        public BusinessObjects.AppUser? GetById(int userId)
        {
            return _userRepository.GetById(userId);
        }

        public void UpdateProfile(UpdateProfileDTO dto)
        {
            var user = _userRepository.GetById(dto.UserId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            if (!string.IsNullOrWhiteSpace(dto.DisplayName))
            {
                var existing = _userRepository.GetByDisplayName(dto.DisplayName);

                if (existing != null && existing.UserId != dto.UserId)
                {
                    throw new Exception("DisplayName already exists");
                }
            }

            user.DisplayName = dto.DisplayName;
            user.AvatarUrl = dto.AvatarUrl;
            user.Bio = dto.Bio;
            user.PhoneNumber = dto.PhoneNumber;
            user.City = dto.City;
            user.District = dto.District;

            _userRepository.Update(user);
        }

        public void ChangePassword(int userId, string oldPassword, string newPassword)
        {
            var user = _userRepository.GetById(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, oldPassword);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                throw new Exception("Old password is incorrect");
            }

            if (oldPassword == newPassword)
            {
                throw new Exception("New password must be different from old password");
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            _userRepository.Update(user);
        }


    }
}
