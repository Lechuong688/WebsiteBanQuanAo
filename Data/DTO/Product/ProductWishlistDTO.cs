using Data.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.Product
{
    public class ProductWishlistDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public string? Name { get; set; }
        public string? TypeName { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; }
        [JsonProperty("Files"), JsonConverter(typeof(SafeCollectionConverter))]
        public List<string> Files { get; set; } = new();
        public string? FilesRaw { get; set; }
        public int TotalRecord { get; set; }
        public int? DiscountPercent { get; set; }
        public decimal FinalPrice { get; set; }
        public bool IsInWishlist { get; set; }
    }
}
