using Data.DTO.Dashboard;
using Data.Repository;
using Data.Repository.Dashboard;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.SqlClient;

public class DashboardRepository : IDashboardRepository
{
    private readonly DataContext _context;
    private readonly IDatabaseSql _databaseSql;
    public DashboardRepository(DataContext context, IDatabaseSql databaseSql)
    {
        _context = context;
        _databaseSql = databaseSql;
    }

    public async Task<DashboardSummaryDTO> GetSummary()
    {
        var result = await _databaseSql.ExecuteProcToList<DashboardSummaryDTO>(
            "Dashboard_GetSummary",
            new List<SqlParameter>()
        ) ?? new List<DashboardSummaryDTO>();

        return result.FirstOrDefault() ?? new DashboardSummaryDTO();
    }

    public async Task<List<RevenueByMonthDTO>> GetRevenueByMonth(
    DateTime startDate,
    DateTime endDate,
    string mode)
    {
        var par = new List<SqlParameter>()
    {
        new SqlParameter("@FromDate", startDate),
        new SqlParameter("@ToDate", endDate),
        new SqlParameter("@GroupBy", mode)
    };

        var result = await _databaseSql.ExecuteProcToList<RevenueByMonthDTO>(
            "Dashboard_RevenueFilterFull",
            par
        );

        return result?.ToList()
               ?? new List<RevenueByMonthDTO>();
    }

    public async Task<List<OrderStatusDTO>> GetOrderStatus(
    DateTime startDate,
    DateTime endDate)
    {
        var par = new List<SqlParameter>()
    {
        new SqlParameter("@FromDate", startDate),
        new SqlParameter("@ToDate", endDate)
    };

        var result = await _databaseSql.ExecuteProcToList<OrderStatusDTO>(
            "Dashboard_GetOrderStatus",
            par
        );

        return result?.ToList()
               ?? new List<OrderStatusDTO>();
    }

    public async Task<List<TopSellingProductDTO>> GetTopSellingProducts(
    DateTime startDate,
    DateTime endDate, int top)
    {
        var par = new List<SqlParameter>()
    {
        new SqlParameter("@FromDate", startDate),
        new SqlParameter("@ToDate", endDate),
        new SqlParameter("@Top", top)
    };

        var result = await _databaseSql.ExecuteProcToList<TopSellingProductDTO>(
            "Dashboard_GetTopSellingProducts",
            par
        );

        return result?.ToList()
               ?? new List<TopSellingProductDTO>();
    }

    public async Task<List<RecentOrderDTO>> GetRecentOrders(
    DateTime startDate,
    DateTime endDate, int top)
    {
        var par = new List<SqlParameter>()
    {
        new SqlParameter("@FromDate", startDate),
        new SqlParameter("@ToDate", endDate),
        new SqlParameter("@Top", top)
    };

        var result = await _databaseSql.ExecuteProcToList<RecentOrderDTO>(
            "Dashboard_GetRecentOrders",
            par
        );

        return result?.ToList()
               ?? new List<RecentOrderDTO>();
    }

    public async Task<List<LowStockProductDTO>> GetLowStockProducts(int quantity)
    {
        var par = new List<SqlParameter>()
        {
            new SqlParameter("@Quantity", quantity)
        };
        var result = await _databaseSql.ExecuteProcToList<LowStockProductDTO>(
            "Dashboard_GetLowStockProducts",
            par
        );

        return result?.ToList() ?? new List<LowStockProductDTO>();
    }
}