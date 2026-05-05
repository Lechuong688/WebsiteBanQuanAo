using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.Product
{
    public class CreateProductReviewReplyDTO
    {
        public int ProductReviewId { get; set; }

        public string? UserId { get; set; }

        public string? Content { get; set; }
    }
}
