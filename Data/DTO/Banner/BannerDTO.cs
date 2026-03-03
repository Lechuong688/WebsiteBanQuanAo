using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO.Banner
{
    public class BannerDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [MaxLength(250)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string? SubTitle { get; set; }

        [MaxLength(100)]
        public string? ButtonText { get; set; }

        public int? CollectionId { get; set; }

        [Display(Name = "Thứ tự hiển thị")]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
