using Data.DTO.Dashboard;
using Data.Repository;
using Data.Repository.Dashboard;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<RevenueByMonthDTO>> GetRevenueByMonth(DateTime startDate, DateTime endDate)
    {
        var par = new List<SqlParameter>()
    {
        new SqlParameter("@StartDate", startDate),
        new SqlParameter("@EndDate", endDate)
    };

        var result = await _databaseSql.ExecuteProcToList<RevenueByMonthDTO>(
            "Dashboard_RevenueSummary",
            par
        ) ?? new List<RevenueByMonthDTO>();

        return result?.ToList() ?? new List<RevenueByMonthDTO>();
    }

    public async Task<List<OrderStatusDTO>> GetOrderStatus()
    {
        var result = await _databaseSql.ExecuteProcToList<OrderStatusDTO>(
            "Dashboard_GetOrderStatus",
            new List<SqlParameter>()
        );

        return result?.ToList() ?? new List<OrderStatusDTO>();
    }
}