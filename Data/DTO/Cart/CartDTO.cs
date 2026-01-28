using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.Cart
{
    public class CartDTO
    {
        public List<CartItemDTO> Items { get; set; } = new();

        public decimal SubTotal { get; set; }

        //public decimal ShippingFee { get; set; }

        public decimal Total { get; set; }
    }
}
