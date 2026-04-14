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
            // 1. Tiền tố Shop của bạn
            string prefix = "VB";

            // 2. Chuỗi ngày tháng (yyMMdd)
            string datePart = DateTime.Now.ToString("yyMMdd");

            // 3. Random 4 ký tự (Đã cố tình loại bỏ các số 0, 1 và chữ O, I để khách không nhìn nhầm)
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            string randomPart = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // 4. Ghép lại thành mã hoàn chỉnh (VD: VB260414A7K9)
            return $"{prefix}{datePart}{randomPart}";
        }
    }
}
