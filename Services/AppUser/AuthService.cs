using BusinessObjects;
using Microsoft.AspNetCore.Identity;
using Repositories.AppUser;
using Services.AppUser;
using Services.DTOs;
using AppUserEntity = BusinessObjects.AppUser;

namespace Services.Auth
{
    public class AuthService: IAuthService
    {

        private readonly IAppUserRepository _userRepository;
        private readonly IEmailVerificationRepository _emailVerificationRepository;
        private readonly IEmailService _emailService;

        public AuthService(
            IAppUserRepository userRepository,
            IEmailVerificationRepository emailVerificationRepository,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _emailVerificationRepository = emailVerificationRepository;
            _emailService = emailService;
        }

        public void Register(RegisterUserDTO dto)
        {
            var existingEmail = _userRepository.GetByEmail(dto.Email);
            if (existingEmail != null)
            {
                throw new Exception("Email already exists");
            }

            var existingUserName = _userRepository.GetByUserName(dto.UserName);
            if (existingUserName != null)
            {
                throw new Exception("Username already exists");
            }

            var existingPhone = _userRepository.GetByPhone(dto.PhoneNumber);
            if (existingPhone != null)
            {
                throw new Exception("Phone number already exists");
            }

            var existingDisplayName = _userRepository.GetByDisplayName(dto.DisplayName);

            if (existingDisplayName != null)
            {
                throw new Exception("Display name already exists");
            }

            var user = new AppUserEntity
            {
                Email = dto.Email.Trim(),
                UserName = dto.UserName.Trim(),
                PhoneNumber = dto.PhoneNumber,
                DisplayName = dto.DisplayName,
                Bio = dto.Bio,
                City = dto.City,
                District = dto.District,
                RoleId = 2,
                EmailConfirmed = false,
                IsBanned = false,
                AvatarUrl = "/images/default-avatar.png",
                SkillLevel = 1,
                CreatedAt = DateTime.Now
            };

            var passwordHasher = new PasswordHasher<AppUserEntity>();
            user.PasswordHash = passwordHasher.HashPassword(user, dto.Password);

            _userRepository.Add(user);

            _emailVerificationRepository.RemoveOldOtpByEmail(dto.Email);

            string otp = new Random().Next(100000, 999999).ToString();

            var emailVerification = new EmailVerification
            {
                Email = dto.Email.Trim(),
                OTP = otp,
                ExpireTime = DateTime.Now.AddMinutes(5),
                IsUsed = false,
                CreatedAt = DateTime.Now
            };

            _emailVerificationRepository.Add(emailVerification);

            _emailService.SendOtp(dto.Email.Trim(), otp);
        }

        public void VerifyOtp(VerifyOtpDTO dto)
        {
            var user = _userRepository.GetByEmail(dto.Email);
            if (user == null)
            {
                throw new Exception("Account not found");
            }

            if (user.EmailConfirmed)
            {
                throw new Exception("Email already verified");
            }

            var otpRecord = _emailVerificationRepository.GetLatestUnusedByEmail(dto.Email);
            if (otpRecord == null)
            {
                throw new Exception("OTP not found");
            }

            if (otpRecord.ExpireTime < DateTime.Now)
            {
                throw new Exception("OTP has expired");
            }

            if (otpRecord.OTP != dto.OTP.Trim())
            {
                throw new Exception("OTP is incorrect");
            }

            otpRecord.IsUsed = true;
            _emailVerificationRepository.Update(otpRecord);

            user.EmailConfirmed = true;
            user.UpdatedAt = DateTime.Now;
            _userRepository.Update(user);
        }

        public void ResendOtp(string email)
        {
            var user = _userRepository.GetByEmail(email);
            if (user == null)
            {
                throw new Exception("Account not found");
            }

            if (user.EmailConfirmed)
            {
                throw new Exception("Email already verified");
            }

            _emailVerificationRepository.RemoveOldOtpByEmail(email);

            string otp = new Random().Next(100000, 999999).ToString();

            var emailVerification = new EmailVerification
            {
                Email = email.Trim(),
                OTP = otp,
                ExpireTime = DateTime.Now.AddMinutes(5),
                IsUsed = false,
                CreatedAt = DateTime.Now
            };

            _emailVerificationRepository.Add(emailVerification);

            _emailService.SendOtp(email.Trim(), otp);
        }
        public AppUserEntity Login(LoginDTO dto)
        {
            var user = _userRepository.GetByEmail(dto.Email);

            if (user == null)
            {
                throw new Exception("Email or password is incorrect");
            }

            if (user.IsBanned)
            {
                throw new Exception("Your account has been banned");
            }

            if (!user.EmailConfirmed)
            {
                throw new Exception("Your email has not been verified");
            }

            var passwordHasher = new PasswordHasher<AppUserEntity>();
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                throw new Exception("Email or password is incorrect");
            }

            return user;
        }
    }
}
