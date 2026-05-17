using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.Dashboard
{
    public class DashboardViewModel
    {
        public DashboardSummaryDTO Summary { get; set; }

        public List<RevenueByMonthDTO> RevenueChart { get; set; } = new();

        public List<OrderStatusDTO> OrderStatus { get; set; } = new();

        public List<TopSellingProductDTO> TopProducts { get; set; } = new();

        public List<RecentOrderDTO> RecentOrders { get; set; } = new();

        public List<LowStockProductDTO> LowStockProducts { get; set; } = new();
    }
}
