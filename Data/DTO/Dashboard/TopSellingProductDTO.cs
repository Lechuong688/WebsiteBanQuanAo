using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.Dashboard
{
    public class TopSellingProductDTO
    {
        public int Id { get; set; }

        public string ProductName { get; set; }

        public int QuantitySold { get; set; }

        public decimal Revenue { get; set; }

        public string ImagePath { get; set; }
    }
}
