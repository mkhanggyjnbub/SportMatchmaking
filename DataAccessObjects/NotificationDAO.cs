using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessObjects
{
    public class NotificationDAO
    {
        private readonly SportMatchmakingContext _context;

        public NotificationDAO(SportMatchmakingContext context)
        {
            _context = context;
        }

        public void Add(Notification entity)
        {
            _context.Notifications.Add(entity);
        }

        public Notification? GetById(long notificationId, int userId)
        {
            return _context.Notifications
                .FirstOrDefault(x => x.NotificationId == notificationId && x.UserId == userId);
        }

        public List<Notification> GetRecentByUser(int userId, int take)
        {
            return _context.Notifications
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(take)
                .AsNoTracking()
                .ToList();
        }

        public int GetUnreadCount(int userId)
        {
            return _context.Notifications
                .Count(x => x.UserId == userId && !x.IsRead);
        }

        public void MarkAsRead(long notificationId, int userId)
        {
            var entity = _context.Notifications
                .FirstOrDefault(x => x.NotificationId == notificationId && x.UserId == userId);

            if (entity != null && !entity.IsRead)
            {
                entity.IsRead = true;
                entity.ReadAt = DateTime.UtcNow;
            }
        }

        public void MarkAllAsRead(int userId)
        {
            var items = _context.Notifications
                .Where(x => x.UserId == userId && !x.IsRead)
                .ToList();

            if (items.Count == 0)
            {
                return;
            }

            var now = DateTime.UtcNow;
            foreach (var item in items)
            {
                item.IsRead = true;
                item.ReadAt = now;
            }
        }

        /// <summary>
        /// Xóa toàn bộ thông báo của user (không phải soft delete).
        /// </summary>
        public void DeleteAllForUser(int userId)
        {
            var items = _context.Notifications
                .Where(x => x.UserId == userId)
                .ToList();

            if (items.Count == 0)
            {
                return;
            }

            _context.Notifications.RemoveRange(items);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}

