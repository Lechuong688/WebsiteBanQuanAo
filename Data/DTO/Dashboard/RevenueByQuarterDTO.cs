using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.Dashboard
{
    public class RevenueByQuarterDTO
    {
        public int Quarters { get; set; }
        public decimal Years { get; set; }
        public int TotalOrder { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
