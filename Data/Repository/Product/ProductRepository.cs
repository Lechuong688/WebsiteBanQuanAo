using Data.DTO.Attribute;
using Data.DTO.Common;
using Data.DTO.Product;
using Data.Entity;
using Microsoft.CodeAnalysis;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.Product
{
    public class ProductRepository : IProductRepository
    {
        private readonly DataContext _context;
        private readonly IDatabaseSql _databaseSql;
        public ProductRepository(DataContext context, IDatabaseSql databaseSql)
        {
            _context = context;
            _databaseSql = databaseSql;
        }

        //GetAllProductAdmin
        public IEnumerable<ProductListDTO> GetAll()
        {
            return (
            from p in _context.Product
            join md in _context.MasterData
                on p.TypeId equals md.Id
            join parent in _context.MasterData
                on md.GroupId equals parent.Id

            where !p.IsDeleted
                  && !md.IsDeleted
                  && !parent.IsDeleted

            let discountPercent = (
                from pd in _context.ProductDiscount
                join d in _context.Discount
                    on pd.DiscountId equals d.Id
                where pd.ProductId == p.Id
                      && d.IsActive
                      && (d.StartDate == null || d.StartDate <= DateTime.Now)
                      && (d.EndDate == null || d.EndDate >= DateTime.Now)
                select (int?)d.Percent
            ).Max()

            select new ProductListDTO
            {
                Id = p.Id,
                Name = p.Name,
                Quantity = p.Quantity,
                Price = p.Price,

                DiscountPercent = discountPercent,

                FinalPrice = discountPercent != null
                    ? p.Price - (p.Price * discountPercent.Value / 100)
                    : p.Price,

                Note = p.Note,

                TypeId = p.TypeId,
                TypeName = md.Name,
                ParentTypeName = parent.Name,

                Sizes = (from pa in _context.ProductAttribute
                         join md2 in _context.MasterData
                         on pa.ValueId equals md2.Id
                         where pa.ProductId == p.Id
                         && md2.GroupId == 19
                         && !md2.IsDeleted
                         select md2.Name).ToList(),

                Colors = (from pa in _context.ProductAttribute
                          join md3 in _context.MasterData
                          on pa.ValueId equals md3.Id
                          where pa.ProductId == p.Id
                          && md3.GroupId == 18
                          && !md3.IsDeleted
                          select md3.Name).ToList(),

                Files = _context.Attachment
                                .Where(a => a.EntityId == p.Id
                                && a.EntityType == "Product"
                                && a.FilePath != null
                                && a.IsDeleted != true)
                                .Select(a => a.FilePath!)
                                .ToList()
            }).ToList();
        }

        //GetAllProduct và phân trang
        public PagedResult<ProductListDTO> GetForShopPaged(int page, int pageSize, int? typeId = null,
            List<int>? colorIds = null, decimal? maxPrice = null, string? keyword = null, string? sort = null)
        {
            var query =
                from p in _context.Product
                join md in _context.MasterData on p.TypeId equals md.Id
                where !p.IsDeleted && !md.IsDeleted
                select new { p, md };

            if (typeId.HasValue)
            {
                query = query.Where(x => x.p.TypeId == typeId.Value);
            }

            if (colorIds != null && colorIds.Any())
            {
                query = query.Where(x =>
                    _context.ProductAttribute.Any(pa =>
                        pa.ProductId == x.p.Id &&
                        colorIds.Contains(pa.ValueId)
                    ));
            }

            if (maxPrice.HasValue && maxPrice.Value > 0)
            {
                query = query.Where(x => x.p.Price <= maxPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.p.Name.Contains(keyword));
            }

            query = sort switch
            {
                "price_asc" => query.OrderBy(x => x.p.Price),
                "price_desc" => query.OrderByDescending(x => x.p.Price),
                "name_asc" => query.OrderBy(x => x.p.Name),
                "name_desc" => query.OrderByDescending(x => x.p.Name),
                "newest" => query.OrderByDescending(x => x.p.CreatedDate),
                _ => query.OrderByDescending(x => x.p.Id)
            };

            var total = query.Count();

            var items = query
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToList()
    .Select(x =>
    {
        var discount = (
            from pd in _context.ProductDiscount
            join d in _context.Discount
                on pd.DiscountId equals d.Id
            where pd.ProductId == x.p.Id
                && d.IsActive
                && (d.StartDate == null || d.StartDate <= DateTime.Now)
                && (d.EndDate == null || d.EndDate >= DateTime.Now)
            select d.Percent
        ).DefaultIfEmpty().Max();

        return new ProductListDTO
        {
            Id = x.p.Id,
            Name = x.p.Name,
            Price = x.p.Price,
            TypeName = x.md.Name,
            Note = x.p.Note,

            DiscountPercent = discount == 0 ? null : discount,

            FinalPrice = discount > 0
                ? x.p.Price - (x.p.Price * discount / 100)
                : x.p.Price,

            Files = _context.Attachment
                .Where(a => a.EntityId == x.p.Id
                    && a.EntityType == "Product"
                    && a.IsDeleted != true)
                .Select(a => a.FilePath!)
                .ToList()
        };
    })
    .ToList();

            return new PagedResult<ProductListDTO>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total
            };
        }

        public async Task<PagedResult<ProductListDTO>> GetList(int page, int pageSize)
        {
            var par = new List<SqlParameter>()
            {
                     new SqlParameter("@Page", page),
                     new SqlParameter("@PageSize", pageSize),
            };
            var result = await _databaseSql.ExecuteProcToList<ProductListDTO>("Product_GetList", par) ?? new List<ProductListDTO>();

            foreach (var item in result)
            {
                if (!string.IsNullOrWhiteSpace(item.FilesRaw))
                    item.Files = item.FilesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

                if (!string.IsNullOrWhiteSpace(item.ColorsRaw))
                    item.Colors = item.ColorsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

                if (!string.IsNullOrWhiteSpace(item.SizesRaw))
                    item.Sizes = item.SizesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            return new PagedResult<ProductListDTO>
            {
                Items = result?.ToList() ?? new List<ProductListDTO>(),
                Page = page,
                PageSize = pageSize,
                TotalItems = result?.FirstOrDefault()?.TotalRecord ?? 0,
            };
        }

        public async Task<ProductListDTO?> GetPinned()
        {
            var result = await _databaseSql.ExecuteProcToList<ProductListDTO>(
                "Product_GetPinned",
                new List<SqlParameter>());

            var product = result?.FirstOrDefault();

            if (product != null && !string.IsNullOrWhiteSpace(product.FilesRaw))
            {
                product.Files = product.FilesRaw
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }

            return product;
        }

        public async Task SetPinned(int id)
        {
            var par = new List<SqlParameter>()
    {
        new SqlParameter("@Id", id)
    };

            await _databaseSql.ExecuteProcToList<int>("Product_SetPinned", par);
        }
        public ProductUpdateDTO? GetById(int id)
        {
            var product = _context.Product.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
            if (product == null)
            {
                return null;
            }

            return new ProductUpdateDTO
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Quantity = product.Quantity,
                Note = product.Note,
                TypeId = product.TypeId,
                IsPinned = product.IsPinned,

                ColorIds = _context.ProductAttribute
                .Where(x => x.ProductId == id)
                .Join(_context.MasterData,
                      pa => pa.ValueId,
                      md => md.Id,
                      (pa, md) => new { pa, md })
                .Where(x => x.md.GroupId == 18)
                .Select(x => x.pa.ValueId)
                .ToList(),

                SizeIds = _context.ProductAttribute
                .Where(x => x.ProductId == id)
                .Join(_context.MasterData,
                      pa => pa.ValueId,
                      md => md.Id,
                      (pa, md) => new { pa, md })
                .Where(x => x.md.GroupId == 19)
                .Select(x => x.pa.ValueId)
                .ToList(),

                ImagePaths = _context.Attachment
                .Where(x => x.EntityId == id
                && x.EntityType == "Product"
                && x.FilePath != null && x.IsDeleted != true)
                .Select(x => x.FilePath!).ToList()
            };
        }

        public ProductEntity Save(ProductUpdateDTO dto)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                ProductEntity product;

                if (dto.Id > 0)
                {
                    product = _context.Product
                        .FirstOrDefault(p => p.Id == dto.Id && p.IsDeleted != true);

                    if (product == null)
                        throw new Exception("Sản phẩm không tồn tại");

                    product.Name = dto.Name;
                    product.Price = dto.Price;
                    product.Quantity = dto.Quantity;
                    product.Note = dto.Note;
                    product.TypeId = dto.TypeId;
                    product.IsPinned = dto.IsPinned;
                    product.UpdatedBy = dto.UserId;
                    product.UpdatedDate = DateTime.Now;
                }
                else
                {
                    product = new ProductEntity
                    {
                        Name = dto.Name,
                        Price = dto.Price,
                        Quantity = dto.Quantity,
                        Note = dto.Note,
                        TypeId = dto.TypeId,
                        IsPinned = dto.IsPinned,
                        IsDeleted = false,
                        CreatedBy = dto.UserId,
                        CreatedDate = DateTime.Now
                    };

                    _context.Product.Add(product);
                    _context.SaveChanges();
                }
                var oldAttrs = _context.ProductAttribute
                    .Where(x => x.ProductId == product.Id);

                _context.ProductAttribute.RemoveRange(oldAttrs);

                var attributeIds = dto.ColorIds
                    .Concat(dto.SizeIds)
                    .Distinct();

                var newAttrs = attributeIds.Select(id => new ProductAttributeEntity
                {
                    ProductId = product.Id,
                    ValueId = id
                });

                _context.ProductAttribute.AddRange(newAttrs);

                if (dto.DeletedImageIds != null && dto.DeletedImageIds.Any())
                {
                    var deletedImages = _context.Attachment
                        .Where(a => dto.DeletedImageIds.Contains(a.Id) && a.IsDeleted != true);


                    foreach (var img in deletedImages)
                    {
                        var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.FilePath.TrimStart('/'));

                        if (File.Exists(physicalPath))
                        {
                            File.Delete(physicalPath);
                        }

                        img.IsDeleted = true;
                    }
                }

                if (dto.ImagePaths != null && dto.ImagePaths.Any())
                {
                    var newImages = dto.ImagePaths.Select(path => new AttachmentEntity
                    {
                        EntityId = product.Id,
                        EntityType = "Product",
                        FilePath = path,
                        FileName = Path.GetFileName(path),
                        IsDeleted = false,
                        CreatedDate = DateTime.Now
                    });

                    _context.Attachment.AddRange(newImages);
                }

                _context.SaveChanges();
                transaction.Commit();

                return product;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<AttachmentEntity> GetImagesByProductId(int productId)
        {
            return _context.Attachment
                .Where(a =>
                    a.EntityId == productId &&
                    a.EntityType == "Product" &&
                    a.IsDeleted != true)
                .ToList();
        }


        public void Delete(int id)
        {
            var product = _context.Product
                .FirstOrDefault(p => p.Id == id && !p.IsDeleted);

            if (product == null)
                throw new Exception("Sản phẩm không tồn tại");

            product.IsDeleted = true;
            product.UpdatedDate = DateTime.Now;

            var oldAttrs = _context.ProductAttribute
                .Where(a => a.ProductId == id);
            _context.ProductAttribute.RemoveRange(oldAttrs);

            var images = _context.Attachment
                .Where(x => x.EntityId == id
                         && x.EntityType == "Product"
                         && x.IsDeleted != true)
                .ToList();

            foreach (var img in images)
            {
                img.IsDeleted = true;
            }

            _context.SaveChanges();

            foreach (var img in images)
            {
                if (string.IsNullOrEmpty(img.FilePath)) continue;

                var physicalPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    img.FilePath.TrimStart('/')
                );

                if (File.Exists(physicalPath))
                    File.Delete(physicalPath);
            }
        }

        public ProductListDTO? GetForDelete(int id)
        {
            return _context.Product
                .Where(p => p.Id == id && !p.IsDeleted)
                .Select(p => new ProductListDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    Note = p.Note
                })
                .FirstOrDefault();
        }


        public List<CategoryDTO> GetCategories()
        {
            return _context.Product
                .Where(p => !p.IsDeleted)
                .Join(_context.MasterData,
                      p => p.TypeId,
                      md => md.Id,
                      (p, md) => new { p, md })
                .Where(x => !x.md.IsDeleted)
                .GroupBy(x => new { x.md.Id, x.md.Name })
                .Select(g => new CategoryDTO
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,
                    ProductCount = g.Count()
                })
                .OrderBy(x => x.Name)
                .ToList();
        }

        public List<CategoryDTO> GetColors()
        {
            return (
                from pa in _context.ProductAttribute
                join p in _context.Product
                    on pa.ProductId equals p.Id
                join md in _context.MasterData
                    on pa.ValueId equals md.Id
                where !p.IsDeleted
                      && !md.IsDeleted
                      && md.GroupId == 18
                group p by new { md.Id, md.Name } into g
                select new CategoryDTO
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,
                    ProductCount = g
                        .Select(x => x.Id)
                        .Distinct()
                        .Count()
                }
            )
            .OrderBy(x => x.Name)
            .ToList();
        }
        public async Task<ProductDetailDTO?> GetDetail(int id)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter("@Id", id)
            };

            var ds = await _databaseSql.ExecuteProcXmlToList<ProductDetailDTO>(
                "Product_GetDetail_XML",
                param
            );
            return ds?.FirstOrDefault() ?? new ProductDetailDTO();
        }

        public PagedResult<ProductListDTO> GetForCollectionPaged(string collectionCode, int page, int pageSize, int? typeId = null,
        List<int>? colorIds = null, decimal? maxPrice = null, string? keyword = null, string? sort = null)
        {
            var now = DateTime.Now;

            var query =
                from p in _context.Product
                join pc in _context.ProductCollection on p.Id equals pc.ProductId
                join c in _context.Collection on pc.CollectionId equals c.Id
                join md in _context.MasterData on p.TypeId equals md.Id
                where !p.IsDeleted
                      && !md.IsDeleted
                      && c.Code == collectionCode
                select new { p, md };

            if (typeId.HasValue) query = query.Where(x => x.p.TypeId == typeId.Value);

            if (colorIds != null && colorIds.Any())
            {
                query = query.Where(x => _context.ProductAttribute.Any(pa =>
                    pa.ProductId == x.p.Id && colorIds.Contains(pa.ValueId)));
            }

            if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(x => x.p.Name.Contains(keyword));

            query = sort switch
            {
                "price_asc" => query.OrderBy(x => x.p.Price),
                "price_desc" => query.OrderByDescending(x => x.p.Price),
                "name_asc" => query.OrderBy(x => x.p.Name),
                "name_desc" => query.OrderByDescending(x => x.p.Name),
                "newest" => query.OrderByDescending(x => x.p.CreatedDate),
                _ => query.OrderByDescending(x => x.p.Id)
            };

            var total = query.Count();

            var rawItems = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.p,
                    x.md,
                    DiscountPercents = _context.ProductDiscount
                        .Where(pd => pd.ProductId == x.p.Id)
                        .Join(_context.Discount, pd => pd.DiscountId, d => d.Id, (pd, d) => d)
                        .Where(d => d.IsActive &&
                                   (d.StartDate == null || d.StartDate <= now) &&
                                   (d.EndDate == null || d.EndDate >= now))
                        .Select(d => d.Percent)
                        .ToList()
                })
                .ToList();

            var items = rawItems.Select(x => {
                int maxDiscount = x.DiscountPercents.Any() ? x.DiscountPercents.Max() : 0;
                decimal finalPrice = maxDiscount > 0
                                     ? x.p.Price - (x.p.Price * maxDiscount / 100)
                                     : x.p.Price;

                return new ProductListDTO
                {
                    Id = x.p.Id,
                    Name = x.p.Name,
                    Price = x.p.Price,
                    TypeName = x.md.Name,
                    Note = x.p.Note,
                    DiscountPercent = maxDiscount,
                    FinalPrice = finalPrice,
                    Files = _context.Attachment
                        .Where(a => a.EntityId == x.p.Id && a.EntityType == "Product" && a.IsDeleted != true)
                        .Select(a => a.FilePath!)
                        .ToList()
                };
            }).ToList();

            if (maxPrice.HasValue && maxPrice.Value > 0)
            {
                items = items.Where(x => x.FinalPrice <= maxPrice.Value).ToList();
            }

            return new PagedResult<ProductListDTO>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total
            };
        }

        public async Task<List<ProductBestsellerDTO>> GetBestseller()
        {
            var result = await _databaseSql.ExecuteProcToList<ProductBestsellerDTO>(
                "Product_Bestseller",
                new List<SqlParameter>()
            );

            return result?.ToList() ?? new List<ProductBestsellerDTO>();
        }

        public async Task<List<ProductTopSellingDTO>> GetTopSelling()
        {
            var result = await _databaseSql.ExecuteProcToList<ProductTopSellingDTO>(
                "Product_TopSelling",
                new List<SqlParameter>()
            );

            return result?.ToList() ?? new List<ProductTopSellingDTO>();
        }

        public async Task<List<ProductNewArrivalDTO>> GetNewArrival()
        {
            var result = await _databaseSql.ExecuteProcToList<ProductNewArrivalDTO>(
                "Product_NewArrival",
                new List<SqlParameter>()
            );

            return result?.ToList() ?? new List<ProductNewArrivalDTO>();
        }
    }
}
