using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.Dashboard
{
    public class DashboardSummaryDTO
    {
        public int TotalOrders { get; set; }

        public decimal TotalRevenue { get; set; }

        public int TotalUsers { get; set; }

        public int ProductsSold { get; set; }
    }
}
