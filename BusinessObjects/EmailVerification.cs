using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class EmailVerification
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;

        public string OTP { get; set; } = null!;

        public bool IsUsed { get; set; }
        public DateTime ExpireTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
