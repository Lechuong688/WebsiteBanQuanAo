using Data.DTO.Attribute;
using Data.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.Product
{
    public class ProductDetailDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string? Note { get; set; }

        public int TypeId { get; set; }
        public string TypeName { get; set; } = null!;
        [JsonProperty("Images"), JsonConverter(typeof(SafeCollectionConverter))]
        public List<string> Images { get; set; } = new();
        [JsonProperty("Colors"), JsonConverter(typeof(SafeCollectionConverter))]
        public List<AttributeDTO> Colors { get; set; } = new();
        [JsonProperty("Sizes"), JsonConverter(typeof(SafeCollectionConverter))]
        public List<AttributeDTO> Sizes { get; set; } = new();
        public int? DiscountPercent { get; set; }
        public decimal FinalPrice { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; }
        public List<ProductReviewListDTO>? Reviews { get; set; }
    }
}
