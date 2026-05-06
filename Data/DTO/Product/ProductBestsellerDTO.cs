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

        public string TypeName { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public decimal? DiscountPercent { get; set; }

        public decimal FinalPrice { get; set; }

        public string Files { get; set; }

        public int TotalSold { get; set; }
        public DateTime CreatedDate { get; set; }
        public double AverageRating { get; set; }

        public int ReviewCount { get; set; }
    }
}
