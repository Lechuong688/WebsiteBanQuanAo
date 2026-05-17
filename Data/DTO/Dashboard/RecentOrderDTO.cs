using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.Dashboard
{
    public class RecentOrderDTO
    {
        public int OrderId { get; set; }

        public string CustomerName { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public string? TransactionCode { get; set; }
    }
}
