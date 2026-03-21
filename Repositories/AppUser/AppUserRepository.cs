using BusinessObjects;
using DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppUserEntity = BusinessObjects.AppUser;

namespace Repositories.AppUser
{
    public class AppUserRepository : IAppUserRepository
    {
        private readonly AppUserDAO _appUserDAO;

        public AppUserRepository(AppUserDAO appUserDAO)
        {
            _appUserDAO = appUserDAO;
        }

        public AppUserEntity? GetByDisplayName(string displayName)
        {
            return _appUserDAO.GetByDisplayName(displayName);
        }
        public AppUserEntity? GetByPhone(string phone)
        {
            return _appUserDAO.GetByPhone(phone);
        }
        public AppUserEntity? GetByEmail(string email)
        {
            return _appUserDAO.GetByEmail(email);
        }

        public AppUserEntity? GetByUserName(string userName)
        {
            return _appUserDAO.GetByUserName(userName);
        }

        public void Add(AppUserEntity user)
        {
            _appUserDAO.Add(user);
        }
        public void Update(AppUserEntity user)
        {
            _appUserDAO.Update(user);
        }
        public AppUserEntity? GetById(int userId) => _appUserDAO.GetById(userId);
        public Role? GetByName(string roleName)
        {
            return _appUserDAO.GetByName(roleName);
        }
    }
}
