using Data.DTO;
using Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.Product
{
    public class ProductRepository : IProductRepository
    {
        private readonly DataContext _context;
        public ProductRepository(DataContext context)
        {
            _context = context;
        }

        //GetAllProductAdmin
        public IEnumerable<ProductListDTO> GetAll()
        {
            return (
            from p in _context.Product
            join md in _context.MasterData
                on p.TypeId equals md.Id
            where !p.IsDeleted
                  && !md.IsDeleted
            select new ProductListDTO
            {
                Id = p.Id,
                Name = p.Name,
                Quantity = p.Quantity,
                Price = p.Price,

                TypeId = p.TypeId,
                TypeName = md.Name,

                Sizes = (from pa in _context.ProductAttribute
                         join md in _context.MasterData
                         on pa.ValueId equals md.Id
                         where pa.ProductId == p.Id
                         && md.GroupId == 19
                         && !md.IsDeleted
                         select md.Name
                                 ).ToList(),
                Colors = (from pa in _context.ProductAttribute
                          join md in _context.MasterData
                      on pa.ValueId equals md.Id
                          where pa.ProductId == p.Id
                      && md.GroupId == 18
                      && !md.IsDeleted
                          select md.Name
                                 ).ToList(),

                Files = _context.Attachment
                                .Where(a => a.EntityId == p.Id
                                && a.EntityType == "Product"
                                && a.FilePath != null && a.IsDeleted != true)
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

            if (maxPrice.HasValue)
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
                _ => query.OrderByDescending(x => x.p.Id) // default
            };

            var total = query.Count();

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ProductListDTO
                {
                    Id = x.p.Id,
                    Name = x.p.Name,
                    Price = x.p.Price,
                    TypeName = x.md.Name,
                    Files = _context.Attachment
                        .Where(a => a.EntityId == x.p.Id && a.IsDeleted != true)
                        .Select(a => a.FilePath!)
                        .ToList()
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
                        .FirstOrDefault(p => p.Id == dto.Id && !p.IsDeleted);

                    if (product == null)
                        throw new Exception("Sản phẩm không tồn tại");

                    product.Name = dto.Name;
                    product.Price = dto.Price;
                    product.Quantity = dto.Quantity;
                    product.UpdatedDate = DateTime.Now;
                }
                else
                {
                    product = new ProductEntity
                    {
                        Name = dto.Name,
                        Price = dto.Price,
                        Quantity = dto.Quantity,
                        IsDeleted = false,
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

        //Xóa sản phẩm
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
                    Quantity = p.Quantity
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
    }
}
