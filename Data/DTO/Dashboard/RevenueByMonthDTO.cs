using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.Dashboard
{
    public class RevenueByMonthDTO
    {
        public string Label { get; set; }

        public decimal Revenue { get; set; }

        public DateTime GroupKey { get; set; }
    }
}
