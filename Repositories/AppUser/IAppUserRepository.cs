using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppUserEntity = BusinessObjects.AppUser;

namespace Repositories.AppUser
{
    public interface IAppUserRepository
    {
        AppUserEntity? GetByEmail(string email);
        AppUserEntity? GetByUserName(string userName);
        AppUserEntity? GetByPhone(string phone);
        AppUserEntity? GetByDisplayName(string displayName);
        AppUserEntity? GetById(int userId);
        Role? GetByName(string roleName);
        void Add(AppUserEntity user);
        void Update(AppUserEntity user);
    }
}
