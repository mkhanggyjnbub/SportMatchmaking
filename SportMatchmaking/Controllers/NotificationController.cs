using Microsoft.AspNetCore.Mvc;
using Services.Notifications;
using SportMatchmaking.Filters;
using SportMatchmaking.Models;
using System;
using System.Linq;
using System.Text.Json;

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
                DataJson = x.DataJson,
                TargetUrl = BuildTargetUrl(x.Type, x.DataJson),
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

        [HttpPost]
        public IActionResult DeleteAll()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            _notificationService.DeleteAllNotifications(userId.Value);
            return Ok();
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        private string? BuildTargetUrl(string type, string? dataJson)
        {
            if (string.IsNullOrWhiteSpace(dataJson))
            {
                return null;
            }

            try
            {
                using var document = JsonDocument.Parse(dataJson);
                if (!document.RootElement.TryGetProperty("postId", out var postIdElement)
                    || !postIdElement.TryGetInt64(out var postId))
                {
                    return null;
                }

                if (string.Equals(type, "JoinRequest.New", StringComparison.OrdinalIgnoreCase))
                {
                    return Url.Action("PostRequests", "JoinRequest", new { postId });
                }

                if (string.Equals(type, "JoinRequest.Accepted", StringComparison.OrdinalIgnoreCase))
                {
                    return Url.Action("Details", "MatchPost", new { id = postId });
                }

                return null;
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}

