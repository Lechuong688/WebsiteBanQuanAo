using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO
{
    public class ProductListDTO
    {
        public int Id { get; set; }
        [DisplayName("Tên sản phẩm")]
        public string Name { get; set; }
        [DisplayName("Số lượng sản phẩm")]
        public int Quantity { get; set; }
        [DisplayName("Giá sản phẩm")]
        public decimal Price { get; set; }
        [DisplayName("Hình ảnh")]
        public List<string> Files { get; set; }
        [DisplayName("Màu sắc")]
        public List<string> Colors { get; set; }
        [DisplayName("Kích thước")]
        public List<string> Sizes { get; set; }
    }
}
