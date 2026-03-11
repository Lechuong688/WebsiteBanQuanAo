using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entity
{
    public class ProductDiscountEntity
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int DiscountId { get; set; }
    }
}
