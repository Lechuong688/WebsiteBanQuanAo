using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entity
{
    public class ProductCollectionEntity
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int CollectionId { get; set; }
        public string? Type { get; set; }
    }
}
