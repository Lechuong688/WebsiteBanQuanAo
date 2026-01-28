using Data.DTO.Cart;
using Data.DTO.CheckOut;
using Data.Entity;
using Data.Repository;
using Microsoft.AspNetCore.Mvc;
using WebBanQuanAo.helpers;
using WebBanQuanAo.Models;

namespace WebBanQuanAo.Controllers
{
    public class CheckOutController : Controller
    {
        private readonly DataContext _context;

        public CheckOutController(DataContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var cart = CartCookieHelper.GetCart(Request);

            if (!cart.Any())
                return RedirectToAction("Index", "Cart");

            var productIds = cart.Select(x => x.ProductId).Distinct().ToList();

            var products = _context.Product
                .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
                .Select(p => new { p.Id, p.Name, p.Price })
                .ToList();

            var masterData = _context.MasterData
                .Where(x => !x.IsDeleted)
                .ToList();

            var cartDto = new Data.DTO.Cart.CartDTO
            {
                Items = (
                    from c in cart
                    join p in products on c.ProductId equals p.Id
                    join color in masterData on c.ColorId equals color.Id
                    join size in masterData on c.SizeId equals size.Id
                    select new Data.DTO.Cart.CartItemDTO
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
                ).ToList()
            };

            cartDto.SubTotal = cartDto.Items.Sum(x => x.Price * x.Quantity);
            cartDto.ShippingFee = 30000;
            //cartDto.Total = cartDto.SubTotal + cartDto.ShippingFee;
            if (cartDto.SubTotal >= 500000)
            {
                cartDto.Total = cartDto.SubTotal;
            }
            else
            {
                cartDto.Total = cartDto.SubTotal + cartDto.ShippingFee;
            }

            return View(new CheckOutViewModel
            {
                Cart = cartDto
            });
        }

        [HttpPost]
        public IActionResult PlaceOrder(CheckOutViewModel model)
        {
            var cart = CartCookieHelper.GetCart(Request);
            if (!cart.Any())
                return BadRequest("Cart empty");

            var order = new OrderEntity
            {
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                Note = model.Note,
                Status = 0,
                CreatedDate = DateTime.Now
            };

            _context.Order.Add(order);
            _context.SaveChanges();

            var details = cart.Select(c => new OrderDetailEntity
            {
                OrderId = order.Id,
                ProductId = c.ProductId,
                Note = $"Color:{c.ColorId} | Size:{c.SizeId}",
                CreatedDate = DateTime.Now
            });

            _context.OrderDetail.AddRange(details);
            _context.SaveChanges();

            CartCookieHelper.ClearCart(Response);

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
