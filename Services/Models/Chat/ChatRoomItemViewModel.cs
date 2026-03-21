//Vinh 

namespace Services.Models.Chat
{
    public class ChatRoomItemViewModel
    {
        public long ThreadId { get; set; }
        public long? PostId { get; set; }
        public string RoomName { get; set; } = "";
        public string LastMessage { get; set; } = "";
        public string LastMessageTimeText { get; set; } = "";
        public int MemberCount { get; set; }
        public bool IsSelected { get; set; }
    }
}