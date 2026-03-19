using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entity.Enums
{
    public enum OrderStatus
    {
        New = 0,       // Mới đặt
        Confirmed = 1, // Đã xác nhận
        Shipping = 2,  // Đang giao
        Completed = 3, // Hoàn thành
        Cancelled = 4  // Đã huỷ
    }
}
