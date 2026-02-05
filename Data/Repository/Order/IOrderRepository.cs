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
        Task<PagedResult<OrderAdminDTO>> GetOrders(int? status, int page, int pageSize);
        Task<OrderAdminDetailDTO?> GetOrderDetail(int orderId);
        Task UpdateStatus(int orderId, int status, string updatedBy);
    }
}
