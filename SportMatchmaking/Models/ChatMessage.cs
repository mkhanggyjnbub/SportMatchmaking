using System;
using System.Collections.Generic;

namespace SportMatchmaking.Models;

public partial class ChatMessage
{
    public long MessageId { get; set; }

    public long ThreadId { get; set; }

    public int SenderUserId { get; set; }

    public string MessageText { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public DateTime? EditedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual AppUser SenderUser { get; set; } = null!;

    public virtual ChatThread Thread { get; set; } = null!;
}
