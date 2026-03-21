using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessObjects
{
    public class AppUserDAO
    {
        private readonly SportMatchmakingContext _context;

        public AppUserDAO(SportMatchmakingContext context)
        {
            this._context = context;
        }
        public AppUser? GetByPhone(string phone)
        {
            return _context.AppUsers
                .FirstOrDefault(x => x.PhoneNumber == phone);
        }

        public AppUser? GetByDisplayName(string displayName)
        {
            return _context.AppUsers
                .FirstOrDefault(x => x.DisplayName == displayName);
        }

        public AppUser? GetByEmail(string email)
        {
            return _context.AppUsers
                .Include(x => x.Role)   
                .FirstOrDefault(x => x.Email.ToLower() == email.Trim().ToLower());
        }

        public AppUser? GetByUserName(string userName)
        {
            return _context.AppUsers
                .FirstOrDefault(x => x.UserName.ToLower() == userName.Trim().ToLower());
        }

        public void Add(AppUser user)
        {
            _context.AppUsers.Add(user);
            _context.SaveChanges();
        }

        public void Update(AppUser user)
        {
            _context.AppUsers.Update(user);
            _context.SaveChanges();
        }
        public AppUser? GetById(int userId)
        {
            return _context.AppUsers.FirstOrDefault(x => x.UserId == userId);
        }
        public Role? GetByName(string roleName)
        {
            return _context.Roles
                .FirstOrDefault(r => r.Name == roleName);
        }
    }
}
