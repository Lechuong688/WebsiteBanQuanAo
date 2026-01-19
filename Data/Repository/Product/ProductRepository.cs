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

        public IEnumerable<ProductListDTO> GetAll()
        {
            return _context.Product.Where(p => !p.IsDeleted)
                .Select(p => new ProductListDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Quantity = p.Quantity,
                    Price = p.Price,

                    Colors = (
                from pa in _context.ProductAttribute
                join md in _context.MasterData
                    on pa.ValueId equals md.Id
                where pa.ProductId == p.Id
                      && md.GroupId == 18
                      && !md.IsDeleted
                select md.Name
                    ).ToList(),

                    Sizes = (from pa in _context.ProductAttribute
                             join md in _context.MasterData
                             on pa.ValueId equals md.Id
                             where pa.ProductId == p.Id
                             && md.GroupId == 19
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
                        var physicalPath = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot",img.FilePath.TrimStart('/'));

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

    }
}
