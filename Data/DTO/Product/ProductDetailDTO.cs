using Data.DTO.Attribute;
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

        public List<string> Images { get; set; } = new();
        public List<AttributeDTO> Colors { get; set; } = new();
        public List<AttributeDTO> Sizes { get; set; } = new();
    }
}
