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
    }
}