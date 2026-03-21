using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs
{
    public class CreateJoinRequestDTO
    {
        public long PostId { get; set; }
        public int RequesterUserId { get; set; }
        public byte? SkillLevel { get; set; }
        public int PartySize { get; set; }
        public string? Message { get; set; }
        public string? GuestNames { get; set; }
    }
}
