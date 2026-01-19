
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entity
{
    public class MasterDataEntity
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Yêu cầu chọn nhóm danh mục")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã nhóm danh mục không hợp lệ")]
        public int GroupId { get; set; }
        [Required, MinLength(2, ErrorMessage = "Yêu cầu nhập Code")]
        public string? Code { get; set; }
        [Required, MinLength(2, ErrorMessage = "Yêu cầu nhập tên danh mục")]
        public string? Name { get; set; }
        public string? Note { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
