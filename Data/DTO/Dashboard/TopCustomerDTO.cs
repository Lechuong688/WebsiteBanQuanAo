using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.Dashboard
{
    public class TopCustomerDTO { 
        public string CustomerName { get; set; } 
        public string PhoneNumber { get; set; } 
        public int TotalOrders { get; set; } 
        public decimal TotalSpent { get; set; } 
        public string AvatarPath { get; set; }
    }
}
