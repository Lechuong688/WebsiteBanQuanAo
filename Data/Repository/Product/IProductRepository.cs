using Data.DTO;
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
        IEnumerable<ProductListDTO> GetAll();
        PagedResult<ProductListDTO> GetForShopPaged(int page, int pageSize, int? typeId = null,
            List<int>? colorIds = null, decimal? maxPrice = null, string? keyword = null, string? sort = null);

        ProductUpdateDTO? GetById(int id);
        List<AttachmentEntity> GetImagesByProductId(int productId);
        ProductEntity Save(ProductUpdateDTO dto);
        void Delete(int id);
        ProductListDTO? GetForDelete(int id);
        List<CategoryDTO> GetCategories();
        List<CategoryDTO> GetColors();

    }
}
