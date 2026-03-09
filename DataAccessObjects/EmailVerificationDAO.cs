using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessObjects
{
    public class EmailVerificationDAO
    {
        private readonly SportMatchmakingContext _context;

        public EmailVerificationDAO(SportMatchmakingContext context)
        {
            _context = context;
        }

        public EmailVerification? GetLatestUnusedByEmail(string email)
        {
            return _context.EmailVerifications
                .Where(x => x.Email.ToLower() == email.Trim().ToLower() && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();
        }

        public void Add(EmailVerification entity)
        {
            _context.EmailVerifications.Add(entity);
            _context.SaveChanges();
        }

        public void Update(EmailVerification entity)
        {
            _context.EmailVerifications.Update(entity);
            _context.SaveChanges();
        }

        public void RemoveOldOtpByEmail(string email)
        {
            var list = _context.EmailVerifications
                .Where(x => x.Email.ToLower() == email.Trim().ToLower() && !x.IsUsed)
                .ToList();

            if (list.Any())
            {
                _context.EmailVerifications.RemoveRange(list);
                _context.SaveChanges();
            }
        }
    }
}
