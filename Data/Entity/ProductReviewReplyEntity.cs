using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entity
{
    public class ProductReviewReplyEntity
    {
        public int Id { get; set; }

        public int ProductReviewId { get; set; }

        public string? UserId { get; set; }

        public string? Content { get; set; }

        public bool IsDeleted { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
        public virtual UserEntity User { get; set; }
        //public virtual ProductEntity Product { get; set; }
        public virtual ProductReviewEntity ProductReview { get; set; }
    }
}