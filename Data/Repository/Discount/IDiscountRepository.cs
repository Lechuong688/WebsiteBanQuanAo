using Data.DTO.Common;
using Data.DTO.Discount;
using Data.DTO.Product;
using Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.Discount
{
    public interface IDiscountRepository
    {
        Task<PagedResult<DiscountListDTO>> GetList(int page, int pageSize);
        DiscountEntity GetById(int id);
        void Save(DiscountEntity entity);
        void Delete(int id);
        List<int> GetProductIds(int discountId);
        void SaveProducts(int discountId, List<int> productIds);
        Task<List<ProductListDTO>> GetProductsByDiscount(int discountId);
}
}
