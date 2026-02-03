using Data.DTO.CheckOut;
using Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.Order
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DataContext _context;
        public OrderRepository(DataContext context)
        {
            _context = context;
        }
        public int CreateOrder(OrderCreateDTO dto)
        {
            if (dto.Items == null || !dto.Items.Any())
                throw new Exception("Danh sách sản phẩm trống");

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var productIds = dto.Items
                    .Select(x => x.ProductId)
                    .Distinct()
                    .ToList();

                var prices = _context.Product
                    .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
                    .Select(p => new { p.Id, p.Price })
                    .ToDictionary(x => x.Id, x => x.Price);

                decimal subTotal = dto.Items.Sum(x =>
                    prices[x.ProductId] * x.Quantity
                );

                decimal shippingFee = subTotal >= 500000 ? 0 : 30000;
                decimal total = subTotal + shippingFee;

                var order = new OrderEntity
                {
                    UserId = dto.UserId,
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    Address = dto.Address,
                    Note = dto.Note,

                    SubTotal = subTotal,
                    ShippingFee = shippingFee,
                    Total = total,

                    Status = 0,
                    CreatedDate = DateTime.Now
                };

                _context.Order.Add(order);
                _context.SaveChanges();

                var orderDetails = dto.Items.Select(i => new OrderDetailEntity
                {
                    OrderId = order.Id,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    ColorId = i.ColorId,
                    SizeId = i.SizeId,
                    CreatedDate = DateTime.Now
                }).ToList();

                _context.OrderDetail.AddRange(orderDetails);
                _context.SaveChanges();

                transaction.Commit();
                return order.Id;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
