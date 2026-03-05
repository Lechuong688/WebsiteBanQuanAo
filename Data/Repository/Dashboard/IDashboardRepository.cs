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
        Task<List<RevenueByMonthDTO>> GetRevenueByMonth(int year);
        Task<List<OrderStatusDTO>> GetOrderStatus();
    }
}
