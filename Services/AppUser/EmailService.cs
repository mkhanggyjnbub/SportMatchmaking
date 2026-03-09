using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Services.AppUser
{
    public class EmailService: IEmailService
    {
        public void SendOtp(string toEmail, string otp)
        {
            var fromEmail = "baolnqce180242@fpt.edu.vn";
            var appPassword = "jyjq usnc cjyu vbbe";

            using var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(fromEmail, appPassword),
                EnableSsl = true
            };

            using var mail = new MailMessage();
            mail.From = new MailAddress(fromEmail, "SportMatchmaking");
            mail.To.Add(toEmail);
            mail.Subject = "Verify your email";
            mail.Body = $"Your OTP code is: {otp}. This code will expire in 5 minutes.";

            smtp.Send(mail);
        }
    }
}
