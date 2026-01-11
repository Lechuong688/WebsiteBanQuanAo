using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entity
{
    public class RoleEntity : IdentityRole
    {
        //[Key]
        //public int Id { get; set; }
        [Required, MinLength(2, ErrorMessage = "Yêu cầu nhập tên")]
        public string? Name { get; set; }
        public string? Note { get; set; }
    }
}
