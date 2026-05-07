using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.Product
{
    public class ProductReviewProductDTO
    {
        public int ProductId { get; set; }

        public string? ProductName { get; set; }

        public string? Thumbnail { get; set; }

        public double AverageRating { get; set; }

        public int TotalReview { get; set; }
    }
}
