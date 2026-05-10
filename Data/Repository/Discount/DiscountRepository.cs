using Data.DTO.Common;
using Data.DTO.Discount;
using Data.DTO.Product;
using Data.Entity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.Discount
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly DataContext _context;
        private readonly IDatabaseSql _databaseSql;
        public DiscountRepository(DataContext context, IDatabaseSql databaseSql)
        {
            _context = context;
            _databaseSql = databaseSql;
        }

        public async Task<PagedResult<DiscountListDTO>> GetList(int page, int pageSize)
        {
            var par = new List<SqlParameter>()
            {
                     new SqlParameter("@Page", page),
                     new SqlParameter("@PageSize", pageSize),
            };
            var result = await _databaseSql.ExecuteProcToList<DiscountListDTO>("Discount_Admin_GetList", par) ?? new List<DiscountListDTO>();

            return new PagedResult<DiscountListDTO>
            {
                Items = result?.ToList() ?? new List<DiscountListDTO>(),
                Page = page,
                PageSize = pageSize
            };
        }

        public DiscountEntity GetById(int id)
        {
            return _context.Discount
                .FirstOrDefault(x => x.Id == id);
        }

        public void Save(DiscountEntity entity)
        {
            if (entity.Id == 0)
            {
                entity.CreatedDate = DateTime.Now;
                entity.IsActive = true;

                _context.Discount.Add(entity);
            }
            else
            {
                var dbEntity = _context.Discount
                                           .FirstOrDefault(x => x.Id == entity.Id);

                if (dbEntity == null) return;

                dbEntity.Name = entity.Name;
                dbEntity.Percent = entity.Percent;
                dbEntity.StartDate = entity.StartDate;
                dbEntity.EndDate = entity.EndDate;
                dbEntity.IsActive = entity.IsActive;
                dbEntity.UpdatedBy = entity.UpdatedBy;
                dbEntity.UpdatedDate = DateTime.Now;
            }

            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.Discount
                                     .FirstOrDefault(x => x.Id == id);

            if (entity == null) return;

            entity.IsActive = false;
            entity.UpdatedDate = DateTime.Now;

            _context.Discount.Update(entity);
            _context.SaveChanges();
        }

        public List<int> GetProductIds(int discountId)
        {
            return _context.ProductDiscount
                .Where(x => x.DiscountId == discountId)
                .Select(x => x.ProductId)
                .ToList();
        }

        public void SaveProducts(int discountId, List<int> productIds)
        {
            var oldData = _context.ProductDiscount
                .Where(x => x.DiscountId == discountId);

            _context.ProductDiscount.RemoveRange(oldData);

            if (productIds != null && productIds.Any())
            {
                var newData = productIds.Select(pid => new ProductDiscountEntity
                {
                    DiscountId = discountId,
                    ProductId = pid
                });

                _context.ProductDiscount.AddRange(newData);
            }
            _context.SaveChanges();
        }

        public async Task<List<ProductListDTO>> GetProductsByDiscount(int discountId)
        {
            var param = new List<SqlParameter>
    {
        new SqlParameter("@DiscountId", discountId)
    };

            var result = await _databaseSql.ExecuteProcXmlToList<ProductListDTO>(
                "Product_GetByDiscount",
                param
            );

            return result?.ToList() ?? new List<ProductListDTO>();
        }

        public async Task<PagedResult<DiscountCodeListDTO>> GetListDiscountCode(int page, int pageSize)
        {
            var par = new List<SqlParameter>()
            {
                     new SqlParameter("@Page", page),
                     new SqlParameter("@PageSize", pageSize),
            };
            var result = await _databaseSql.ExecuteProcToList<DiscountCodeListDTO>("DiscountCode_Admin_GetList", par) ?? new List<DiscountCodeListDTO>();

            return new PagedResult<DiscountCodeListDTO>
            {
                Items = result?.ToList() ?? new List<DiscountCodeListDTO>(),
                Page = page,
                PageSize = pageSize
            };
        }

        public DiscountCodeEntity GetByIdDiscountCode(int id)
        {
            return _context.DiscountCode.FirstOrDefault(x => x.Id == id);
        }

        public void SaveDiscountCode(DiscountCodeEntity discountCode)
        {
            if (discountCode.Id == 0)
            {
                discountCode.CreatedDate = DateTime.Now;
                discountCode.IsActive = true;

                _context.DiscountCode.Add(discountCode);
            }
            else
            {
                var dbEntity = _context.DiscountCode
                    .FirstOrDefault(x => x.Id == discountCode.Id);

                if (dbEntity == null) return;

                dbEntity.Code = discountCode.Code;
                dbEntity.Name = discountCode.Name;
                dbEntity.DiscountType = discountCode.DiscountType;
                dbEntity.DiscountValue = discountCode.DiscountValue;
                dbEntity.MinOrderValue = discountCode.MinOrderValue;
                dbEntity.MaxDiscount = discountCode.MaxDiscount;
                dbEntity.Quantity = discountCode.Quantity;
                dbEntity.UsedCount = discountCode.UsedCount;
                dbEntity.StartDate = discountCode.StartDate;
                dbEntity.EndDate = discountCode.EndDate;
                dbEntity.UpdatedBy = discountCode.UpdatedBy;
                dbEntity.UpdatedDate = DateTime.Now;
                dbEntity.IsActive = discountCode.IsActive;
            }
            _context.SaveChanges();
        }

        public void DeleteDiscountCode(int id)
        {
            var discountCode = _context.DiscountCode
                                     .FirstOrDefault(x => x.Id == id);

            if (discountCode == null) return;

            discountCode.IsActive = false;
            discountCode.UpdatedDate = DateTime.Now;

            _context.DiscountCode.Update(discountCode);
            _context.SaveChanges();
        }
    }
}
