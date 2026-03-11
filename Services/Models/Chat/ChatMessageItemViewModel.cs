//Vinh 

namespace Services.Models.Chat
{
    public class ChatMessageItemViewModel
    {
        public long MessageId { get; set; }
        public long ThreadId { get; set; }
        public int SenderUserId { get; set; }
        public string SenderName { get; set; } = "";
        public string MessageText { get; set; } = "";
        public DateTime SentAt { get; set; }
        public bool IsMine { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? EditedAt { get; set; }
        public bool CanEditOrDelete { get; set; }
    }
}