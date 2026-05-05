using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.Product
{
    public class CreateProductReviewDTO
    {
        public int ProductId { get; set; }

        public string? UserId { get; set; }

        public int OrderId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }
        public List<string>? ImagePaths { get; set; }
    }
}
