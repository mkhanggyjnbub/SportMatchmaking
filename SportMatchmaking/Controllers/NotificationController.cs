using Microsoft.AspNetCore.Mvc;
using Services.Notifications;
using SportMatchmaking.Filters;
using SportMatchmaking.Models;
using System;
using System.Linq;

namespace SportMatchmaking.Controllers
{
    [UserOnly]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public IActionResult Count()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { unreadCount = 0 });
            }

            var count = _notificationService.GetUnreadCount(userId.Value);
            return Json(new { unreadCount = count });
        }

        [HttpGet]
        public IActionResult Dropdown()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var items = _notificationService.GetRecentNotifications(userId.Value, 10);

            var vm = items.Select(x => new NotificationItemVM
            {
                NotificationId = x.NotificationId,
                Title = x.Title,
                Body = x.Body,
                Type = x.Type,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt
            }).ToList();

            return PartialView("~/Views/Notification/_NotificationDropdown.cshtml", vm);
        }

        [HttpPost]
        public IActionResult MarkAllRead()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            _notificationService.MarkAllAsRead(userId.Value);
            return Ok();
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }
    }
}

