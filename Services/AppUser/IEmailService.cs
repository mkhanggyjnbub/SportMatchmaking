using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.AppUser
{
    public interface IEmailService
    {
        void SendOtp(string toEmail, string otp);
    }
}
