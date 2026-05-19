using Data.DTO.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.Dashboard
{
    public interface IDashboardRepository
    {
        Task<DashboardSummaryDTO> GetSummary();
        Task<List<RevenueByMonthDTO>> GetRevenueByMonth(DateTime startDate, DateTime endDate, string mode);
        Task<List<TopSellingProductDTO>> GetTopSellingProducts(
    DateTime startDate,
    DateTime endDate, int top);

        Task<List<RecentOrderDTO>> GetRecentOrders(
            DateTime startDate,
            DateTime endDate, int top);

        Task<List<OrderStatusDTO>> GetOrderStatus(
            DateTime startDate,
            DateTime endDate);

        Task<List<LowStockProductDTO>> GetLowStockProducts(int quantity);

        Task<List<TopCustomerDTO>> GetTopCustomers(DateTime startDate, DateTime endDate, int top);

        Task<BestShoppingTimeDTO> GetBestShoppingTime(DateTime startDate, DateTime endDate);

        Task<List<OrderStatisticDTO>>GetOrderStatistic(DateTime startDate, DateTime endDate, string mode);
    }
}
