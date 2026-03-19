using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entity
{
    public class DiscountCodeEntity
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public int DiscountType { get; set; }
        public decimal DiscountValue { get; set; }

        public decimal? MinOrderValue { get; set; }
        public decimal? MaxDiscount { get; set; }

        public int Quantity { get; set; }
        public int UsedCount { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
