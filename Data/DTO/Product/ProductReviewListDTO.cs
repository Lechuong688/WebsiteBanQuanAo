using Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.Product
{
    public class ProductReviewListDTO
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public virtual UserEntity User { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime? CreatedDate { get; set; }
        public List<string>? Images { get; set; }
        public List<ProductReviewReplyListDTO>? Replies { get; set; }
    }
}
