using Data.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.Product
{
    //public class ProductListDTO
    //{
    //    public int Id { get; set; }
    //    [DisplayName("Tên sản phẩm")]
    //    public string Name { get; set; }
    //    [DisplayName("Số lượng sản phẩm")]
    //    public int Quantity { get; set; }
    //    [DisplayName("Giá sản phẩm")]
    //    public decimal Price { get; set; }
    //    public string? Note { get; set; }
    //    [DisplayName("Hình ảnh")]
    //    public List<string> Files { get; set; } = new();
    //    [DisplayName("Màu sắc")]
    //    public List<string> Colors { get; set; } = new();
    //    [DisplayName("Kích thước")]
    //    public List<string> Sizes { get; set; } = new();
    //    public int TypeId { get; set; }
    //    public string? TypeName { get; set; }
    //    [DisplayName("Danh mục cha")]
    //    public string? ParentTypeName { get; set; }
    //    public int TotalRecord { get; set; }

    //}
    public class ProductListDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? Note { get; set; }
        [JsonProperty("Files"), JsonConverter(typeof(SafeCollectionConverter))]
        public List<string> Files { get; set; } = new();
        public List<string> Colors { get; set; } = new();
        public List<string> Sizes { get; set; } = new();

        public string? FilesRaw { get; set; }
        public string? ColorsRaw { get; set; }
        public string? SizesRaw { get; set; }

        public int TypeId { get; set; }
        public string? TypeName { get; set; }
        public string? ParentTypeName { get; set; }

        public int TotalRecord { get; set; }
        public int? DiscountPercent { get; set; }
        public decimal FinalPrice { get; set; }
    }

}
