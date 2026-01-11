using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entity
{
    public class ProductEntity
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Mã loại sản phẩm không được để trống")]
        public int TypeId { get; set; }
        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string? Name { get; set; }
        public string? Note { get; set; }
        public bool IsDeleted { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}
