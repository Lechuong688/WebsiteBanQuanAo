using Data.Entity;
using Data.Entity.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace WebBanQuanAo.Areas.Admin.Models
{
    //public class MasterDataViewModel
    //{
    //    public int Id { get; set; }

    //    [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
    //    public string Name { get; set; }
    //    public string Code { get; set; }
    //    public string? Note { get; set; }

    //    public int ParentCategoryId { get; set; } = 0;

    //    [ValidateNever]
    //    public List<MasterDataEntity> ParentCategories { get; set; }
    //}

    public class MasterDataViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string? Note { get; set; }
        public int ParentId { get; set; }

        public MasterDataType Type { get; set; }

        public List<MasterDataEntity> ParentItems { get; set; }
    }

}
