using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO
{
    public class ProductUpdateDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public List<int>? ColorIds { get; set; } = new();
        public List<int>? SizeIds { get; set; } = new();
        public List<string> ImagePaths { get; set; } = new();
        //public List<ImageDTO> Images { get; set; }
        public List<int> DeletedImageIds { get; set; } = new();
    }

}
