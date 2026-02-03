using Data.DTO.CheckOut;
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
        int CreateOrder(OrderCreateDTO dto);
    }
}
