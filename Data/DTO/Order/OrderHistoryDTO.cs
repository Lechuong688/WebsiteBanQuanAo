using System;
using System.Collections.Generic;

namespace Data.DTO.Order
{
    // Cục bọc ngoài: Thông tin chung của Đơn hàng
    public class OrderHistoryDTO
    {
        public int Id { get; set; }
        public string TransactionCode { get; set; }
        public int Status { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedDate { get; set; }

        // Danh sách các sản phẩm bên trong đơn
        public List<OrderHistoryDetailDTO> Details { get; set; } = new List<OrderHistoryDetailDTO>();
    }

    // Cục bọc trong: Chi tiết từng sản phẩm
    public class OrderHistoryDetailDTO
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Image { get; set; }
        public string ColorName { get; set; }
        public string SizeName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}