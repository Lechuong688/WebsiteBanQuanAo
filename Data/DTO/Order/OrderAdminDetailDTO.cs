using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.Order
{
    public class OrderAdminDetailDTO
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }

        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total { get; set; }

        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public decimal? DiscountAmount { get; set; }
        public string TransactionCode { get; set; }
        public List<OrderItemAdminDTO> Items { get; set; } = new();
    }
}
