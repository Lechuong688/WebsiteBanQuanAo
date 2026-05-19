using Data.DTO.CheckOut;
using Data.DTO.Common;
using Data.DTO.Order;
using Data.Entity;
using Data.Helper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.Order
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DataContext _context;
        private readonly IDatabaseSql _databaseSql;
        public OrderRepository(DataContext context, IDatabaseSql databaseSql)
        {
            _context = context;
            _databaseSql = databaseSql;
        }

        public async Task<int> CreateOrder(OrderCreateDTO dto)
        {
            dto.TransactionCode = OrderHelper.GenerateTransactionCode();

            var json = JsonConvert.SerializeObject(dto);
            var xmlData = JsonConvert.DeserializeXmlNode(json, "XMLData");
            var param = new List<SqlParameter>
            {
                new SqlParameter("@XMLData", xmlData.InnerXml),
            };

            var result = await _databaseSql.ExecuteProcNonQuery(
                "Order_Save",
                param
            );
            return result;
        }
        public async Task<PagedResult<OrderAdminDTO>> GetOrders(
    int? status,
    string keyword,
    DateTime? fromDate,
    DateTime? toDate,
    int page,
    int pageSize)
        {
            var param = new List<SqlParameter>
    {
        new SqlParameter("@Status", status ?? (object)DBNull.Value),
        new SqlParameter("@Keyword", string.IsNullOrEmpty(keyword) ? (object)DBNull.Value : keyword),
        new SqlParameter("@FromDate", fromDate ?? (object)DBNull.Value),
        new SqlParameter("@ToDate", toDate ?? (object)DBNull.Value),
        new SqlParameter("@Page", page),
        new SqlParameter("@PageSize", pageSize)
    };

            var result = await _databaseSql
                .ExecuteProcXmlToList<OrderAdminDTO>("Order_Admin_GetList", param)
                ?? new List<OrderAdminDTO>();

            return new PagedResult<OrderAdminDTO>
            {
                Items = result.ToList(),
                Page = page,
                PageSize = pageSize,
                TotalItems = result.FirstOrDefault()?.TotalRecord ?? 0
            };
        }


        public async Task<OrderAdminDetailDTO?> GetOrderDetail(int orderId)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter("@OrderId", orderId)
            };

            var order = (await _databaseSql.ExecuteProcXmlToList<OrderAdminDetailDTO>(
                "Order_Admin_GetDetail",
                param
            ))?.FirstOrDefault();

            if (order == null)
                return null;

            order.Items = (await _databaseSql.ExecuteProcXmlToList<OrderItemAdminDTO>(
            "Order_Admin_GetItems",
            param
            ))?.ToList() ?? new List<OrderItemAdminDTO>();

            return order;
        }

        public async Task UpdateStatus(int orderId, int status, string updatedBy)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@Status", status),
                new SqlParameter("@UpdatedBy", updatedBy),
            };

            await _databaseSql.ExecuteProcNonQuery(
                "Order_Admin_UpdateStatus",
                param
            );
        }
        public List<OrderHistoryDTO> GetOrderHistory(string userId, List<int> guestOrderIds)
        {
            var query = _context.Order.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(o => o.UserId == userId);
            else if (guestOrderIds != null && guestOrderIds.Any())
                query = query.Where(o => guestOrderIds.Contains(o.Id));
            else
                return new List<OrderHistoryDTO>();

            var orders = query.OrderByDescending(o => o.CreatedDate).ToList();
            var orderIds = orders.Select(o => o.Id).ToList();

            if (!orderIds.Any()) return new List<OrderHistoryDTO>();

            var details = (from od in _context.Set<OrderDetailEntity>()
                           join p in _context.Product on od.ProductId equals p.Id
                           join mc in _context.MasterData on od.ColorId equals mc.Id into colorGrp
                           from color in colorGrp.DefaultIfEmpty()
                           join ms in _context.MasterData on od.SizeId equals ms.Id into sizeGrp
                           from size in sizeGrp.DefaultIfEmpty()
                           where orderIds.Contains(od.OrderId)
                           select new OrderHistoryDetailDTO
                           {
                               OrderId = od.OrderId,
                               ProductId = od.ProductId,
                               ProductName = p.Name,
                               Quantity = od.Quantity,
                               Price = od.Price,
                               ColorName = color != null ? color.Name : "",
                               SizeName = size != null ? size.Name : "",
                               Image = _context.Attachment
                                    .Where(a => a.EntityId == p.Id && a.EntityType == "Product" && a.IsDeleted != true)
                                    .Select(a => a.FilePath)
                                    .FirstOrDefault()
                           }).ToList();

            var result = orders.Select(o => new OrderHistoryDTO
            {
                Id = o.Id,
                TransactionCode = o.TransactionCode,
                Status = o.Status,
                Total = o.Total,
                CreatedDate = o.CreatedDate ?? DateTime.Now,
                Details = details.Where(d => d.OrderId == o.Id).ToList()
            }).ToList();

            return result;
        }

        public Data.Entity.OrderEntity GetOrderById(int id)
        {
            return _context.Order.FirstOrDefault(o => o.Id == id);
        }

        public Data.Entity.OrderEntity GetOrderByWebhookContent(string content)
        {
            return _context.Order
                .FirstOrDefault(x => content.Contains(x.TransactionCode.ToUpper()));
        }

        public void UpdateOrderStatus(int orderId, int status)
        {
            var order = _context.Order.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.Status = status;
                _context.SaveChanges();
            }
        }
        public async Task CancelExpiredOrders()
        {
            var expireTime = DateTime.Now.AddHours(-24);

            var expiredOrders = await _context.Order
                .Where(o => o.Status == 0 && o.CreatedDate < expireTime)
                .ToListAsync();

            if (expiredOrders.Any())
            {
                foreach (var order in expiredOrders)
                {
                    order.Status = 4;
                    order.Note = (order.Note ?? "") + " | Hệ thống tự động hủy do quá 24h chưa thanh toán.";
                    order.UpdatedDate = DateTime.Now;
                    order.UpdatedBy = "System";
                }
                await _context.SaveChangesAsync();

            }
        }
    }
}
