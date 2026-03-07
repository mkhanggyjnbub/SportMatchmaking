using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class ChatThread
{
    public long ThreadId { get; set; }

    public byte ThreadType { get; set; }

    public long? PostId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ICollection<ChatThreadMember> ChatThreadMembers { get; set; } = new List<ChatThreadMember>();

    public virtual MatchPost? Post { get; set; }
}
