using Data.DTO.CheckOut;
using Data.DTO.Common;
using Data.DTO.Order;
using Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.Order
{
    public interface IOrderRepository
    {
        Task<int> CreateOrder(OrderCreateDTO dto);
        Task<PagedResult<OrderAdminDTO>> GetOrders(int? status, string keyword,
            DateTime? fromDate, DateTime? toDate, int page, int pageSize);
        Task<OrderAdminDetailDTO?> GetOrderDetail(int orderId);
        Task UpdateStatus(int orderId, int status, string updatedBy);
        List<OrderHistoryDTO> GetOrderHistory(string userId, List<int> guestOrderIds);
        Data.Entity.OrderEntity GetOrderById(int id);
        Data.Entity.OrderEntity GetOrderByWebhookContent(string content);
        void UpdateOrderStatus(int orderId, int status);
        Task CancelExpiredOrders();
    }
}
