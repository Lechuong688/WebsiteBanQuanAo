using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entity
{
    public class ProductReviewEntity
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string? UserId { get; set; }

        public int OrderId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public bool IsApproved { get; set; }

        public bool IsDeleted { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}