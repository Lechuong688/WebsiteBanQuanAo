using Data.DTO.Cart;
using Data.DTO.CheckOut;
using Data.Entity;
using Data.Helper;
using Data.Repository;
using Data.Repository.Order;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using WebBanQuanAo.helpers;
using WebBanQuanAo.Models;

namespace WebBanQuanAo.Controllers
{
    public class CheckOutController : Controller
    {
        private readonly DataContext _context;
        private readonly IOrderRepository _orderRepository;

        public CheckOutController(DataContext context, IOrderRepository orderRepository)
        {
            _context = context;
            _orderRepository = orderRepository;
        }
        public IActionResult Index()
        {
            var selectedItems = Request.Query["SelectedItems"];
            var cart = CartCookieHelper.GetCart(Request);
            var selectedList = selectedItems.Select(x => x.ToString()).ToList();

            if (selectedItems.Any())
            {
                cart = cart.Where(x =>
                    selectedList.Contains($"{x.ProductId}-{x.ColorId}-{x.SizeId}")
                ).ToList();
            }
            if (!cart.Any())
                return RedirectToAction("Index", "Cart");

            var productIds = cart.Select(x => x.ProductId).Distinct().ToList();

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

                    Price = discount != null
                        ? p.Price - (p.Price * discount.Value / 100)
                        : p.Price
                }
            ).ToList();

            var masterData = _context.MasterData
                .Where(x => !x.IsDeleted)
                .ToList();

            var cartDto = new CartDTO
            {

                Items = (
                    from c in cart
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
                ).ToList()
            };

            cartDto.SubTotal = cartDto.Items.Sum(x => x.Price * x.Quantity);
            cartDto.ShippingFee = 30000;
            cartDto.Total = cartDto.SubTotal >= 500000
                ? cartDto.SubTotal
                : cartDto.SubTotal + cartDto.ShippingFee;

            var model = new CheckOutViewModel
            {
                Cart = cartDto,
                IsAuthenticated = User.Identity != null && User.Identity.IsAuthenticated,
                SelectedItems = selectedList
            };

            if (model.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = _context.Users.FirstOrDefault(x => x.Id == userId);
                model.FullName = user.Name;
                model.Email = user.Email;
                model.PhoneNumber = user.PhoneNumber;

                if (user != null)
                {
                    model.FullName = user.Name;
                    model.Email = user.Email;
                    model.PhoneNumber = user.PhoneNumber;
                    //model.Address = user.Address;
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckOutViewModel model)
        {
            var action = Request.Form["action"];
            var cart = CartCookieHelper.GetCart(Request);
            var selectedItems = Request.Form["SelectedItems"];
            var selectedList = selectedItems.Select(x => x.ToString()).ToList();
            var paymentMethod = Request.Form["PaymentMethod"];

            if (selectedList.Any())
            {
                cart = cart.Where(x =>
                    selectedList.Contains($"{x.ProductId}-{x.ColorId}-{x.SizeId}")
                ).ToList();
            }
            Console.WriteLine("Selected count: " + selectedList.Count);
            if (!cart.Any())
                return RedirectToAction("Index", "Cart");

            string? userId = null;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            string fullName = model.FullName;
            string email = model.Email;
            string phone = model.PhoneNumber;

            if(userId != null)
            {
                var user = _context.Users.FirstOrDefault(x => x.Id == userId);
                if (user == null)
                    return Unauthorized();

                fullName = string.IsNullOrWhiteSpace(fullName) ? user.Name : fullName;
                email = string.IsNullOrWhiteSpace(email) ? user.Email : email;
                phone = string.IsNullOrWhiteSpace(phone) ? user.PhoneNumber : phone;
            }

            if(string.IsNullOrWhiteSpace(fullName)||
            string.IsNullOrWhiteSpace(email)||
            string.IsNullOrWhiteSpace(phone))

            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin người nhận");
                return RedirectToAction("Index");
            }

            var dto = new OrderCreateDTO
            {
                UserId = userId,
                FullName = fullName,
                Email = email,
                PhoneNumber = phone,
                Address = model.Address,
                Note = model.Note,
                CreatedBy = userId,

                Items = cart.Select(x => new OrderItemDTO
                {
                    ProductId = x.ProductId,
                    ColorId = x.ColorId,
                    SizeId = x.SizeId,
                    Quantity = x.Quantity
                }).ToList()
            };

            decimal discountAmount = 0;
            var discountCode = _context.DiscountCode
                .FirstOrDefault(x => x.Code == model.DiscountCode);

            if (discountCode != null
                && discountCode.IsActive
                && (discountCode.StartDate == null || discountCode.StartDate <= DateTime.Now)
                && (discountCode.EndDate == null || discountCode.EndDate >= DateTime.Now)
                && discountCode.UsedCount < discountCode.Quantity)
            {
                var productIds = cart.Select(x => x.ProductId).Distinct().ToList();

                var products = _context.Product
                .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
                .ToList();

                decimal subTotal = 0;

                foreach (var item in cart)
                {
                    var product = products.First(p => p.Id == item.ProductId);
                    subTotal += product.Price * item.Quantity;
                }

                if (!discountCode.MinOrderValue.HasValue || subTotal >= discountCode.MinOrderValue.Value)
                {
                    if (discountCode.DiscountType == 1)
                    {
                        discountAmount = subTotal * discountCode.DiscountValue / 100;

                        if (discountCode.MaxDiscount.HasValue && discountAmount > discountCode.MaxDiscount)
                        {
                            discountAmount = discountCode.MaxDiscount.Value;
                        }
                    }
                    else
                    {
                        discountAmount = discountCode.DiscountValue;
                    }
                    discountCode.UsedCount += 1;
                    _context.SaveChanges();

                }
            }
            dto.TransactionCode = OrderHelper.GenerateTransactionCode();
            dto.DiscountAmount = discountAmount;
            if (action == "apply")
            {
                var productIds = cart.Select(x => x.ProductId).Distinct().ToList();

                var products = (
                    from p in _context.Product
                    where productIds.Contains(p.Id) && !p.IsDeleted

                    let discount = (
                        from pd in _context.ProductDiscount
                        join d in _context.Discount on pd.DiscountId equals d.Id
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
                        Price = discount != null
                            ? p.Price - (p.Price * discount.Value / 100)
                            : p.Price
                    }
                ).ToList();

                var masterData = _context.MasterData
                .Where(x => !x.IsDeleted)
                .ToList();
                var cartDto = new CartDTO
                {
                    Items = (
                        from c in cart
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
                    ).ToList()
                };

                cartDto.SubTotal = cartDto.Items.Sum(x => x.Price * x.Quantity);
                cartDto.ShippingFee = cartDto.SubTotal >= 500000 ? 0 : 30000;
                cartDto.Total = cartDto.SubTotal + cartDto.ShippingFee - discountAmount;

                if (cartDto.Total < 0)
                    cartDto.Total = 0;

                var modelView = new CheckOutViewModel
                {
                    Cart = cartDto,
                    DiscountAmount = discountAmount,
                    DiscountCode = model.DiscountCode,
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    Note = model.Note,
                    IsAuthenticated = true,
                    SelectedItems = selectedList
                };

                return View("Index", modelView);
            }
            await _orderRepository.CreateOrder(dto);

            var order = _context.Order
                .FirstOrDefault(x => x.TransactionCode == dto.TransactionCode);

            if (order == null)
            {
                return Content("Không tìm thấy đơn sau khi tạo ❌");
            }

            if (string.IsNullOrEmpty(userId))
            {
                var existingCookie = Request.Cookies["GuestOrders"];
                var newCookieValue = string.IsNullOrEmpty(existingCookie)
                    ? order.Id.ToString()
                    : $"{existingCookie},{order.Id}";

                Response.Cookies.Append("GuestOrders", newCookieValue, new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30),
                    HttpOnly = true
                });
            }

            if (paymentMethod == "BANK")
            {
                return RedirectToAction("Payment", "Order", new { id = order.Id });
            }
            else
            {
                var fullCart = CartCookieHelper.GetCart(Request);

                var remainingCart = fullCart
                    .Where(x => !selectedList.Contains($"{x.ProductId}-{x.ColorId}-{x.SizeId}"))
                    .ToList();

                CartCookieHelper.SaveCart(Response, remainingCart);

                return RedirectToAction("Success", new { id = order.Id });
            }
        }

        public IActionResult Success(int id)
        {
            // Tìm lại đơn hàng bằng Repo để lấy TransactionCode
            var order = _orderRepository.GetOrderById(id);

            if (order == null)
            {
                return RedirectToAction("Index", "Home"); // Tránh lỗi nếu ko tìm thấy
            }

            // Truyền mã xịn ra View
            ViewBag.TransactionCode = order.TransactionCode;

            return View();
        }
    }
}
