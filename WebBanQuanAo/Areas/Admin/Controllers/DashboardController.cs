using Data.DTO.Dashboard;
using Data.Entity;
using Data.Helper;
using Data.Repository;
using Data.Repository.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
            var data = await _dashboardRepository.GetSummary();

            return View(data);
        }

        public ActionResult Dashboardv2()
        {
            return View();
        }

        public async Task<IActionResult> GetRevenueChart(int year)
        {
            var data = await _dashboardRepository.GetRevenueByMonth(year);
            return Json(data);
        }

        public async Task<IActionResult> GetOrderStatusChart()
        {
            var data = await _dashboardRepository.GetOrderStatus();
            return Json(data);
        }
    }
}