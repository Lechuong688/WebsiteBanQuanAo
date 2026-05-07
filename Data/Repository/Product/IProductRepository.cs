using Data.DTO.Common;
using Data.DTO.Product;
using Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.Product
{
    public interface IProductRepository
    {
        IEnumerable<ProductListDTO> GetAll(string userId);
        PagedResult<ProductListDTO> GetForShopPaged(int page, int pageSize, int? typeId = null,
            List<int>? colorIds = null, decimal? maxPrice = null, string? keyword = null, string? sort = null, string? userId = null);
        Task<PagedResult<ProductListDTO>> GetList(string userId, string keyword, string size, string color, decimal? minPrice, decimal? maxPrice, int page, int pageSize);
        Task<ProductListDTO?> GetPinned();
        Task SetPinned(int id);

        ProductUpdateDTO? GetById(int id);
        List<AttachmentEntity> GetImagesByProductId(int productId);
        ProductEntity Save(ProductUpdateDTO dto);
        void Delete(int id);
        ProductListDTO? GetForDelete(int id);
        List<CategoryDTO> GetCategories();
        List<CategoryDTO> GetColors();
        Task<ProductDetailDTO?> GetDetail(int id);
        PagedResult<ProductListDTO> GetForCollectionPaged(string collectionCode, int page, int pageSize, int? typeId = null,
            List<int>? colorIds = null, decimal? maxPrice = null, string? keyword = null, string? sort = null);

        Task<List<ProductBestsellerDTO>> GetBestseller();
        Task<List<ProductTopSellingDTO>> GetTopSelling();
        Task<List<ProductNewArrivalDTO>> GetNewArrival(string? userId);

        Task<PagedResult<ProductWishlistDTO>> GetProductWishlist(string userId, int page, int pageSize);

        Task<bool> ToggleWishlist(int productId, string userId);
        Task<List<ProductListDTO>> InstantSearch(string keyword);
    }
}
