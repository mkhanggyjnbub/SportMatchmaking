using Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppUserLogin = BusinessObjects.AppUser;

namespace Services.Auth
{
    public interface IAuthService
    {
        void Register(RegisterUserDTO dto);
        void VerifyOtp(VerifyOtpDTO dto);
        void ResendOtp(string email);
        AppUserLogin Login(LoginDTO dto);
    }
}
