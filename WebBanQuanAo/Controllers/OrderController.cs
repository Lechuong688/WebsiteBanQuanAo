using Data.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using WebBanQuanAo.Models;

namespace WebBanQuanAo.Controllers
{
    public class OrderController : Controller
    {
        private readonly DataContext _context;

        public OrderController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Payment(int id)
        {
            var order = _context.Order.Find(id);

            if (order == null)
            {
                return Content("Không tìm thấy đơn hàng ❌");
            }

            string qrUrl = $"https://img.vietqr.io/image/MB-0372783688-compact2.png" + 
                           $"?amount={(int)order.Total}&addInfo={order.TransactionCode}";

            ViewBag.QR = qrUrl;

            return View(order);
        }
        public JsonResult CheckStatus(int id)
        {
            var order = _context.Order.Find(id);

            if (order == null)
            {
                return Json(new { status = "NotFound" });
            }

            return Json(new { status = order.Status });
        }

        [Route("Order/SepayWebhook")]
        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SepayWebhook()
        {
            try
            {
                Console.WriteLine("==== WEBHOOK HIT ====");

                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();

                Console.WriteLine("RAW: " + body);

                if (string.IsNullOrWhiteSpace(body))
                {
                    Console.WriteLine("BODY NULL ❌");
                    return Ok();
                }

                JObject json;
                try
                {
                    json = JObject.Parse(body);
                }
                catch
                {
                    Console.WriteLine("JSON PARSE FAIL ❌");
                    return Ok();
                }

                string content = json["content"]?.ToString()?.ToUpper() ?? "";

                decimal amount = 0;
                if (json["transferAmount"] != null)
                {
                    decimal.TryParse(json["transferAmount"].ToString(), out amount);
                }

                Console.WriteLine($"Content: {content} | Amount: {amount}");

                if (string.IsNullOrEmpty(content))
                {
                    Console.WriteLine("Content null ❌");
                    return Ok();
                }

                var order = _context.Order
                    .FirstOrDefault(x => content.Contains(x.TransactionCode.ToUpper()));

                if (order != null)
                {
                    order.Status = 1;
                    _context.SaveChanges();

                    Console.WriteLine("Đã update Paid ✅");
                }
                else
                {
                    Console.WriteLine("Không tìm thấy order ❌");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.ToString());
                return Ok();
            }
        }
    }
}
