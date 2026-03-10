//Vinh 

using System.Collections.Generic;

namespace Services.Models.Chat
{
    public class ChatIndexViewModel
    {
        public int CurrentUserId { get; set; }
        public long? SelectedThreadId { get; set; }
        public string SelectedRoomTitle { get; set; } = "";
        public string SelectedRoomSubtitle { get; set; } = "";
        public int SelectedRoomMemberCount { get; set; }

        public List<ChatRoomItemViewModel> Rooms { get; set; } = new();
        public List<ChatMessageItemViewModel> Messages { get; set; } = new();
    }
}
