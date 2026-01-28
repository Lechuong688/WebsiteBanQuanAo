using Data.DTO.Cart;
using Data.Repository;
using Microsoft.AspNetCore.Mvc;
using WebBanQuanAo.helpers;
using WebBanQuanAo.Helpers;
using WebBanQuanAo.Models;

namespace WebBanQuanAo.Controllers
{
    public class CartController : Controller
    {
        private readonly DataContext _context;

        public CartController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cartCookie = CartCookieHelper.GetCart(Request);
            var cartDto = new CartDTO();

            if (!cartCookie.Any())
                return View(cartDto);

            var productIds = cartCookie.Select(x => x.ProductId).Distinct().ToList();

            var products = _context.Product
                .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price
                })
                .ToList();

            var masterData = _context.MasterData
                .Where(x => !x.IsDeleted)
                .ToList();

            cartDto.Items = (
                from c in cartCookie
                join p in products on c.ProductId equals p.Id
                join color in masterData on c.ColorId equals color.Id
                join size in masterData on c.SizeId equals size.Id
                select new CartItemDTO
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    Price = p.Price,
                    Quantity = c.Quantity,

                    ColorId = c.ColorId,
                    ColorName = color.Name,

                    SizeId = c.SizeId,
                    SizeName = size.Name
                }
            ).ToList();

            cartDto.SubTotal = cartDto.Items.Sum(x => x.Price * x.Quantity);
            //cartDto.ShippingFee = 30000;
            cartDto.Total = cartDto.SubTotal;

            return View(cartDto);
        }


        [HttpPost]
        public IActionResult AddToCart(int productId, int colorId, int sizeId, int quantity = 1)
        {
            if (colorId <= 0 || sizeId <= 0)
                return BadRequest("Chưa chọn màu hoặc size");

            var cart = CartCookieHelper.GetCart(Request);

            var item = cart.FirstOrDefault(x =>
                x.ProductId == productId &&
                x.ColorId == colorId &&
                x.SizeId == sizeId);

            if (item != null)
                item.Quantity += quantity;
            else
                cart.Add(new CartItemCookie
                {
                    ProductId = productId,
                    ColorId = colorId,
                    SizeId = sizeId,
                    Quantity = quantity
                });

            CartCookieHelper.SaveCart(Response, cart);
            return Ok();
        }

        [HttpPost]
        public IActionResult Update(int productId, int quantity)
        {
            var cart = CartCookieHelper.GetCart(Request);

            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item == null)
                return BadRequest();

            item.Quantity = quantity < 1 ? 1 : quantity;

            CartCookieHelper.SaveCart(Response, cart);

            var product = _context.Product
                .Where(p => p.Id == productId && !p.IsDeleted)
                .Select(p => p.Price)
                .FirstOrDefault();

            var subTotal = cart.Sum(x =>
            {
                var price = _context.Product
                    .Where(p => p.Id == x.ProductId)
                    .Select(p => p.Price)
                    .FirstOrDefault();

                return price * x.Quantity;
            });

            //var shipping = 30000;
            var total = subTotal;

            return Json(new
            {
                quantity = item.Quantity,
                lineTotal = (item.Quantity * product).ToString("N0"),
                subTotal = subTotal.ToString("N0"),
                total = total.ToString("N0")
            });
        }

        [HttpPost]
        public IActionResult Remove(int productId, int colorId, int sizeId)
        {
            var cart = CartCookieHelper.GetCart(Request);

            cart.RemoveAll(x =>
                x.ProductId == productId &&
                x.ColorId == colorId &&
                x.SizeId == sizeId);

            CartCookieHelper.SaveCart(Response, cart);
            return Ok();
        }
    }
}