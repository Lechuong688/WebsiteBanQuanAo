using Data.Entity.Enums;
using Data.Repository.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebBanQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IActionResult> Index(int? status, int page = 1, int pageSize = 10)
        {
            var result = await _orderRepository.GetOrders(status, page, pageSize);

            ViewBag.Status = status;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            return View(result);
        }


        public async Task<IActionResult> Detail(int id)
        {
            var order = await _orderRepository.GetOrderDetail(id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, int status)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _orderRepository.GetOrderDetail(orderId);

            if (order == null)
                return NotFound();
            if (status <= order.Status)
                return Ok("Không thể quay về trạng thái trước đó!");
            //if (status == order.Status + 2 || status == order.Status + 3 || status == order.Status + 4)   
            //    return Ok("Trạng thái không hợp lệ");

            if (order.Status >= (int)OrderStatus.Shipping &&
            status == (int)OrderStatus.Cancelled)
            {
                return BadRequest("Không thể huỷ đơn khi đang giao hàng");
            }
            await _orderRepository.UpdateStatus(orderId, status, adminId);

            return RedirectToAction("Detail", new { id = orderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int orderId)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue(ClaimTypes.Name);

            var order = await _orderRepository.GetOrderDetail(orderId);
            if (order == null)
                return NotFound();

            if (order.Status >= (int)OrderStatus.Shipping)
                return BadRequest("Không thể hủy đơn ở trạng thái này!");

            await _orderRepository.UpdateStatus(
                orderId,
                (int)OrderStatus.Cancelled,
                adminId
            );

            return RedirectToAction("Detail", new { id = orderId });
        }
    }
}
