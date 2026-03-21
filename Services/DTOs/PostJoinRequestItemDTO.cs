using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs
{
    public class PostJoinRequestItemDTO
    {
        public long RequestId { get; set; }
        public long PostId { get; set; }
        public int RequesterUserId { get; set; }
        public string RequesterName { get; set; } = "";
        public int PartySize { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public byte Status { get; set; }
    }
}
