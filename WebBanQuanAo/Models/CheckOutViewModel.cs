using Data.DTO.Cart;
using System.ComponentModel.DataAnnotations;

namespace WebBanQuanAo.Models
{
    public class CheckOutViewModel
    {
        public bool IsAuthenticated { get; set; }
        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        [Required]
        public string Address { get; set; }

        public string? Note { get; set; }
        public string? CreatedBy { get; set; }

        public CartDTO Cart { get; set; } = new();
        public string DiscountCode { get; set; }
        public decimal? DiscountAmount { get; set; }
    }

}
