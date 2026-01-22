using Data.Entity;

namespace WebBanQuanAo.Areas.Admin.Models
{
    public class UpdateProductViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }

        public List<int> ColorIds { get; set; } = new();
        public List<int> SizeIds { get; set; } = new();

        public List<MasterDataEntity> Colors { get; set; } = new();
        public List<MasterDataEntity> Sizes { get; set; } = new();
        public List<ImageVM> oldImages { get; set; } = new();
        public List<int> DeletedImageIds { get; set; } = new();
        public List<IFormFile>? newImages { get; set; }
        public int TypeId { get; set; }
        public string? TypeName { get; set; }
        public List<MasterDataEntity> Types { get; set; } = new();

    }

    public class ImageVM
    {
        public int Id { get; set; }
        public string Path { get; set; } = null!;
    }

}
