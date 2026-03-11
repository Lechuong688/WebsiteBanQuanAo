using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.Product
{
    public class ProductBestsellerDTO
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public string ProductName { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal FinalPrice { get; set; }
        public int TotalSold { get; set; }
    }
}
