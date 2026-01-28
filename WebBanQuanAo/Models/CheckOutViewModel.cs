using Data.DTO.Cart;
using System.ComponentModel.DataAnnotations;

namespace WebBanQuanAo.Models
{
    public class CheckOutViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Address { get; set; }

        public string? Note { get; set; }

        public CartDTO Cart { get; set; } = new();
    }

}
