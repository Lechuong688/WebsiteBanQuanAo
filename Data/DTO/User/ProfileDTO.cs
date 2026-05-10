using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.User
{
    public class ProfileDTO
    {
        public string Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string? AvatarPath { get; set; }

        public int? AvatarId { get; set; }
    }
}
