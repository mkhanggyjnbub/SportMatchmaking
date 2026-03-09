using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.AppUser
{
    public interface IEmailVerificationRepository
    {
        EmailVerification? GetLatestUnusedByEmail(string email);
        void Add(EmailVerification entity);
        void Update(EmailVerification entity);
        void RemoveOldOtpByEmail(string email);
    }
}
