using BusinessObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _memoryCache;

        private static string PasswordResetCacheKey(string token) => "pwdreset_" + token;

        public AuthService(
            IAppUserRepository userRepository,
            IEmailVerificationRepository emailVerificationRepository,
            IEmailService emailService,
            IMemoryCache memoryCache)
        {
            _userRepository = userRepository;
            _emailVerificationRepository = emailVerificationRepository;
            _emailService = emailService;
            _memoryCache = memoryCache;
        }

        public void Register(RegisterUserDTO dto)
        {
            if (dto == null)
                throw new Exception("Invalid data");

            string email = dto.Email?.Trim() ?? "";
            string userName = dto.UserName?.Trim() ?? "";
            string phoneNumber = dto.PhoneNumber?.Trim() ?? "";
            string displayName = dto.DisplayName?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(email))
                throw new Exception("Email is required");

            if (string.IsNullOrWhiteSpace(userName))
                throw new Exception("Username is required");

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new Exception("Phone number is required");

            if (string.IsNullOrWhiteSpace(displayName))
                throw new Exception("Display name is required");

            var existingUser = _userRepository.GetByEmail(email);

            // Nếu email đã tồn tại và đã xác thực thì chặn
            if (existingUser != null && existingUser.EmailConfirmed)
            {
                throw new Exception("Email already exists");
            }

            // Username đã bị người khác dùng chưa
            var existingUserName = _userRepository.GetByUserName(userName);
            if (existingUserName != null &&
                (existingUser == null || existingUserName.UserId != existingUser.UserId))
            {
                throw new Exception("Username already exists");
            }

            // Phone đã bị người khác dùng chưa
            var existingPhone = _userRepository.GetByPhone(phoneNumber);
            if (existingPhone != null &&
                (existingUser == null || existingPhone.UserId != existingUser.UserId))
            {
                throw new Exception("Phone number already exists");
            }

            // Display name đã bị người khác dùng chưa
            var existingDisplayName = _userRepository.GetByDisplayName(displayName);
            if (existingDisplayName != null &&
                (existingUser == null || existingDisplayName.UserId != existingUser.UserId))
            {
                throw new Exception("Display name already exists");
            }

            var passwordHasher = new PasswordHasher<AppUserEntity>();
            AppUserEntity user;

            // Nếu email đã có nhưng chưa xác thực -> cập nhật lại user cũ
            if (existingUser != null && !existingUser.EmailConfirmed)
            {
                existingUser.UserName = userName;
                existingUser.PhoneNumber = phoneNumber;
                existingUser.DisplayName = displayName;
                existingUser.Bio = dto.Bio;
                existingUser.City = dto.City;
                existingUser.District = dto.District;
                existingUser.PasswordHash = passwordHasher.HashPassword(existingUser, dto.Password);

                _userRepository.Update(existingUser);
                user = existingUser;
            }
            else
            {
                // Email chưa tồn tại -> tạo user mới
                var userRole = _userRepository.GetByName("User");
                if (userRole == null)
                    throw new Exception("Role 'User' does not exist in database");

                user = new AppUserEntity
                {
                    Email = email,
                    UserName = userName,
                    PhoneNumber = phoneNumber,
                    DisplayName = displayName,
                    Bio = dto.Bio,
                    City = dto.City,
                    District = dto.District,
                    RoleId = userRole.RoleId,
                    EmailConfirmed = false,
                    IsBanned = false,
                    AvatarUrl = "/images/default-avatar.png",
                    SkillLevel = 1,
                    CreatedAt = DateTime.Now
                };

                user.PasswordHash = passwordHasher.HashPassword(user, dto.Password);

                _userRepository.Add(user);
            }

            // Xóa OTP cũ
            _emailVerificationRepository.RemoveOldOtpByEmail(email);

            // Tạo OTP mới
            string otp = new Random().Next(100000, 999999).ToString();

            var emailVerification = new EmailVerification
            {
                Email = email,
                OTP = otp,
                ExpireTime = DateTime.Now.AddMinutes(5),
                IsUsed = false,
                CreatedAt = DateTime.Now
            };

            _emailVerificationRepository.Add(emailVerification);

            // Gửi mail
            _emailService.SendOtp(email, otp);
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

        public void RequestPasswordReset(string email)
        {
            var normalized = email?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(normalized))
                return;

            var user = _userRepository.GetByEmail(normalized);
            if (user == null || !user.EmailConfirmed || user.IsBanned)
                return;

            _emailVerificationRepository.RemoveOldOtpByEmail(normalized);

            string otp = new Random().Next(100000, 999999).ToString();

            var emailVerification = new EmailVerification
            {
                Email = normalized,
                OTP = otp,
                ExpireTime = DateTime.Now.AddMinutes(5),
                IsUsed = false,
                CreatedAt = DateTime.Now
            };

            _emailVerificationRepository.Add(emailVerification);
            _emailService.SendPasswordResetOtp(normalized, otp);
        }

        public string VerifyForgotPasswordOtp(string email, string otp)
        {
            var normalized = email?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(normalized))
                throw new Exception("Invalid or expired code");

            var user = _userRepository.GetByEmail(normalized);
            if (user == null || !user.EmailConfirmed)
                throw new Exception("Invalid or expired code");

            if (user.IsBanned)
                throw new Exception("Your account has been banned");

            var otpRecord = _emailVerificationRepository.GetLatestUnusedByEmail(normalized);
            if (otpRecord == null)
                throw new Exception("Invalid or expired code");

            if (otpRecord.ExpireTime < DateTime.Now)
                throw new Exception("Code has expired");

            if (otpRecord.OTP != otp.Trim())
                throw new Exception("Code is incorrect");

            otpRecord.IsUsed = true;
            _emailVerificationRepository.Update(otpRecord);

            var token = Guid.NewGuid().ToString("N");
            _memoryCache.Set(PasswordResetCacheKey(token), normalized, TimeSpan.FromMinutes(15));
            return token;
        }

        public void ResendForgotPasswordOtp(string email)
        {
            var normalized = email?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(normalized))
                throw new Exception("Email is required");

            var user = _userRepository.GetByEmail(normalized);
            if (user == null || !user.EmailConfirmed)
                throw new Exception("Invalid request");

            if (user.IsBanned)
                throw new Exception("Your account has been banned");

            _emailVerificationRepository.RemoveOldOtpByEmail(normalized);

            string otp = new Random().Next(100000, 999999).ToString();

            var emailVerification = new EmailVerification
            {
                Email = normalized,
                OTP = otp,
                ExpireTime = DateTime.Now.AddMinutes(5),
                IsUsed = false,
                CreatedAt = DateTime.Now
            };

            _emailVerificationRepository.Add(emailVerification);
            _emailService.SendPasswordResetOtp(normalized, otp);
        }

        public void CompletePasswordReset(string token, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Reset session expired. Please start again.");

            var cacheKey = PasswordResetCacheKey(token.Trim());
            if (!_memoryCache.TryGetValue(cacheKey, out object? cachedObj) || cachedObj is not string normalizedEmail || string.IsNullOrEmpty(normalizedEmail))
                throw new Exception("Reset session expired. Please start again.");

            var user = _userRepository.GetByEmail(normalizedEmail);
            if (user == null || !user.EmailConfirmed)
                throw new Exception("Reset session expired. Please start again.");

            if (user.IsBanned)
                throw new Exception("Your account has been banned");

            var passwordHasher = new PasswordHasher<AppUserEntity>();
            user.PasswordHash = passwordHasher.HashPassword(user, newPassword);
            user.UpdatedAt = DateTime.Now;
            _userRepository.Update(user);

            _memoryCache.Remove(cacheKey);
        }
    }
}
