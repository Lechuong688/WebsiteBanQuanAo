using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Helper
{
    public static class OrderHelper
    {
        public static string GenerateTransactionCode()
        {
            string prefix = "VB";

            string datePart = DateTime.Now.ToString("yyMMdd");

            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            string randomPart = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return $"{prefix}{datePart}{randomPart}";
        }
    }
}
