using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.Dashboard
{
    public class LowStockProductDTO
    {
        public int Id { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public string ImagePath { get; set; }
    }
}
