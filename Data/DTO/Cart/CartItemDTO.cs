using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.Cart
{
    public class CartItemDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int ColorId { get; set; }
        public string ColorName { get; set; }

        public int SizeId { get; set; }
        public string SizeName { get; set; }
    }
}
