using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.CheckOut
{
    public class OrderItemDTO
    {
        public int ProductId { get; set; }
        public int ColorId { get; set; }
        public int SizeId { get; set; }
        public int Quantity { get; set; }
    }
}
