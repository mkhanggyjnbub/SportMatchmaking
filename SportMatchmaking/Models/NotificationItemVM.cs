using System;

namespace SportMatchmaking.Models
{
    public class NotificationItemVM
    {
        public long NotificationId { get; set; }
        public string Title { get; set; } = "";
        public string? Body { get; set; }
        public string Type { get; set; } = "";
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

