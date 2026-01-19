using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entity
{
    public class ProductAttributeEntity
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int ValueId { get; set; }
        [NotMapped]
        public string? ValueName { get; set; }
        [NotMapped]
        public string? Value { get; set; }
        public string? Type { get; set; }
    }
    public class ProductAttributeDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? ValueId { get; set; }
        public string? ValueName { get; set; }
        public string? Value { get; set; }
        public string? Type { get; set; }
    }
}
