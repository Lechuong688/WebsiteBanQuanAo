using Data.DTO.Dashboard;
using Data.Entity;
using Data.Helper;
using Data.Repository;
using Data.Repository.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;

namespace WebBanQuanAo.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class DashboardController : Controller
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly DataContext _context;
        public DashboardController(IDashboardRepository dashboardRepository, DataContext context)
        {
            _dashboardRepository = dashboardRepository;
            _context = context;
        }
        // GET: Dashboard
        public ActionResult Index()
        {
            return View();
        }

        //public ActionResult Dashboardv1()
        //{
        //    return View();
        //}
        public async Task<IActionResult> Dashboardv1()
        {
            var startDate = DateTime.Now.AddDays(-30);

            var endDate = DateTime.Now;

            var model = new DashboardViewModel
            {
                Summary = await _dashboardRepository.GetSummary(),

                //TopProducts = await _dashboardRepository
                //    .GetTopSellingProducts(startDate, endDate),

                //RecentOrders = await _dashboardRepository
                //    .GetRecentOrders(startDate, endDate),

                LowStockProducts = await _dashboardRepository
                    .GetLowStockProducts(5),

                OrderStatus = await _dashboardRepository
                    .GetOrderStatus(startDate, endDate)
            };

            return View(model);
        }

        public ActionResult Dashboardv2()
        {
            return View();
        }

        public async Task<IActionResult> GetTopProducts(
    DateTime startDate,
    DateTime endDate,
    int top = 5)
        {
            var data = await _dashboardRepository
                .GetTopSellingProducts(startDate, endDate, top);

            return Json(data);
        }
        public async Task<IActionResult> GetRecentOrders(
    DateTime startDate,
    DateTime endDate,
    int top = 5)
        {
            var data = await _dashboardRepository
                .GetRecentOrders(startDate, endDate, top);

            return Json(data);
        }
        public async Task<IActionResult> GetRevenueChart(
    DateTime startDate,
    DateTime endDate,
    string mode = "month")
        {
            var data = await _dashboardRepository
                .GetRevenueByMonth(startDate, endDate, mode);

            return Json(data);
        }

        public async Task<IActionResult> GetOrderStatusChart(
    DateTime startDate,
    DateTime endDate)
        {
            var data = await _dashboardRepository
                .GetOrderStatus(startDate, endDate);

            return Json(data);
        }
        public async Task<IActionResult> GetLowStockProducts(
            int quantity = 5)
        {
            var data = await _dashboardRepository
                .GetLowStockProducts(quantity);

            return Json(data);
        }
    }
}