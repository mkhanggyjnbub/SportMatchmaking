using BusinessObjects;
using DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.AppUser
{
    public class EmailVerificationRepository: IEmailVerificationRepository
    {
        private readonly EmailVerificationDAO _dao;

        public EmailVerificationRepository(EmailVerificationDAO dao)
        {
            _dao = dao;
        }

        public EmailVerification? GetLatestUnusedByEmail(string email)
        {
            return _dao.GetLatestUnusedByEmail(email);
        }

        public void Add(EmailVerification entity)
        {
            _dao.Add(entity);
        }

        public void Update(EmailVerification entity)
        {
            _dao.Update(entity);
        }

        public void RemoveOldOtpByEmail(string email)
        {
            _dao.RemoveOldOtpByEmail(email);
        }
    }
}
