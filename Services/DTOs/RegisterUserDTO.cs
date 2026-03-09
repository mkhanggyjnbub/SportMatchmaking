using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs
{
    public class RegisterUserDTO
    {
        public string Email { get; set; } = null!;

        public string UserName { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string PhoneNumber { get; set; } = "";

        public string DisplayName { get; set; } = "";

        public string? Bio { get; set; }

        public string City { get; set; } = "";

        public string District { get; set; } = "";
    }
}
