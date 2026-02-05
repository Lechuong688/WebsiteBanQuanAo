using Data.DTO.CheckOut;
using Data.DTO.Order;
using Data.DTO.Product;
using Data.Entity;
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

        public async Task<List<OrderAdminDTO>> GetOrders(int? status)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter("@Status", status ?? (object)DBNull.Value)
            };

            var data = await _databaseSql.ExecuteProcXmlToList<OrderAdminDTO>(
                "Order_Admin_GetList",
                param
            );
            return data?.ToList() ?? new List<OrderAdminDTO>();
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
    }
}
