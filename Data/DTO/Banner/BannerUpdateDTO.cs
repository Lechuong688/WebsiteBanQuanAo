using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

namespace WebBanQuanAo.Areas.Admin.Models
{
    public class BannerUpdateDTO
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string? SubTitle { get; set; }
        public string? ButtonText { get; set; }
        public string? Description { get; set; }
        public int CollectionId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }

        public IFormFile? ImageFile { get; set; }
        public string UserId { get; set; }
        public List<SelectListItem>? Collections { get; set; }
        public string? ExistingImagePath { get; set; }
    }
}
