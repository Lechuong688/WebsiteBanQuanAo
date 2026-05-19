using Data.DTO.Cart;
using Data.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
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

            var products = (
                from p in _context.Product
                where productIds.Contains(p.Id) && !p.IsDeleted

                let discount = (
                    from pd in _context.ProductDiscount
                    join d in _context.Discount
                        on pd.DiscountId equals d.Id
                    where pd.ProductId == p.Id
                          && d.IsActive
                          && (d.StartDate == null || d.StartDate <= DateTime.Now)
                          && (d.EndDate == null || d.EndDate >= DateTime.Now)
                    select (int?)d.Percent
                ).Max()

                select new
                {
                    p.Id,
                    p.Name,
                    Image = _context.Attachment
                    .Where(a =>
                        a.EntityId == p.Id &&
                        a.EntityType == "Product" &&
                        a.IsDeleted == false)
                    .Select(a => a.FilePath)
                    .FirstOrDefault(),
                    Price = discount != null
                        ? p.Price - (p.Price * discount.Value / 100)
                        : p.Price
                }
            ).ToList();

            var masterData = _context.MasterData
            .Where(x =>
                !x.IsDeleted &&
                (x.GroupId == 18 || x.GroupId == 19)
            )
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
                    ProductImage = p.Image,
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
        public IActionResult AddToCart(int productId, int colorId, int sizeId, int quantity = 1, bool isBuyNow = false)
        {
            if (colorId <= 0 || sizeId <= 0)
                return BadRequest("Chưa chọn màu hoặc size");

            var cart = CartCookieHelper.GetCart(Request);

            var item = cart.FirstOrDefault(x =>
                x.ProductId == productId &&
                x.ColorId == colorId &&
                x.SizeId == sizeId);

            if (item != null)
            {
                if (isBuyNow)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    item.Quantity += quantity;
                }
            }
            else
            {
                cart.Add(new CartItemCookie
                {
                    ProductId = productId,
                    ColorId = colorId,
                    SizeId = sizeId,
                    Quantity = quantity
                });
            }

            CartCookieHelper.SaveCart(Response, cart);

            return Ok();
        }

        [HttpPost]
        public IActionResult Update(int productId, int colorId, int sizeId, int quantity)
        {
            var cart = CartCookieHelper.GetCart(Request);

            var item = cart.FirstOrDefault(x =>
                x.ProductId == productId &&
                x.ColorId == colorId &&
                x.SizeId == sizeId);

            if (item == null)
                return BadRequest();

            item.Quantity = quantity < 1 ? 1 : quantity;

            CartCookieHelper.SaveCart(Response, cart);

            var productIds = cart.Select(x => x.ProductId).Distinct().ToList();

            var prices = (
                from p in _context.Product
                where productIds.Contains(p.Id) && !p.IsDeleted

                let discount = (
                    from pd in _context.ProductDiscount
                    join d in _context.Discount
                        on pd.DiscountId equals d.Id
                    where pd.ProductId == p.Id
                              && d.IsActive
                              && (d.StartDate == null || d.StartDate <= DateTime.Now)
                              && (d.EndDate == null || d.EndDate >= DateTime.Now)
                    select (int?)d.Percent
                ).Max()

                select new
                {
                    p.Id,
                    Price = discount != null
                        ? p.Price - (p.Price * discount.Value / 100)
                        : p.Price
                }
            )
            .ToList()
            .ToDictionary(x => x.Id, x => x.Price);

            var subTotal = cart.Sum(x => prices[x.ProductId] * x.Quantity);
            var lineTotal = prices[item.ProductId] * item.Quantity;

            return Json(new
            {
                quantity = item.Quantity,
                lineTotal = lineTotal.ToString("N0"),
                subTotal = subTotal.ToString("N0"),
                total = subTotal.ToString("N0")
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