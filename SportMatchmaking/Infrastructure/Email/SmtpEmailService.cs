using Services.AppUser;
using System.Net;
using System.Net.Mail;

namespace SportMatchmaking.Infrastructure.Email
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;
        private readonly IHostEnvironment _env;

        public SmtpEmailService(
            IConfiguration configuration,
            ILogger<SmtpEmailService> logger,
            IHostEnvironment env)
        {
            _configuration = configuration;
            _logger = logger;
            _env = env;
        }

        public void SendOtp(string toEmail, string otp)
        {
            var host = _configuration["Email:SmtpHost"];
            var portStr = _configuration["Email:SmtpPort"];
            var fromEmail = _configuration["Email:FromEmail"];
            var fromName = _configuration["Email:FromName"] ?? "SportMatchmaking";
            var userName = _configuration["Email:UserName"] ?? fromEmail;
            var password = _configuration["Email:Password"];
            var enableSslStr = _configuration["Email:EnableSsl"];

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(fromEmail) ||
                string.IsNullOrWhiteSpace(password) ||
                !int.TryParse(portStr, out var port))
            {
                _logger.LogWarning(
                    "Email SMTP ch?a c?u hình. Không g?i ???c OTP t?i {Email}. OTP (dev): {Otp}",
                    toEmail,
                    otp);
                return;
            }

            bool enableSsl = true;
            if (bool.TryParse(enableSslStr, out var parsedEnableSsl))
            {
                enableSsl = parsedEnableSsl;
            }

            try
            {
                using var smtp = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(userName, password),
                    EnableSsl = enableSsl
                };

                using var mail = new MailMessage();
                mail.From = new MailAddress(fromEmail, fromName);
                mail.To.Add(toEmail);
                mail.Subject = "Verify your email";
                mail.Body = $"Your OTP code is: {otp}. This code will expire in 5 minutes.";

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "G?i OTP th?t b?i t?i {Email}. OTP (dev): {Otp}", toEmail, otp);

                if (_env.IsDevelopment())
                {
                    _logger.LogInformation("DEV OTP for {Email}: {Otp}", toEmail, otp);
                }
            }
        }
    }
}
