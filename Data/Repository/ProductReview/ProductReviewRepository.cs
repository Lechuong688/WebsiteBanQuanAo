using Data.DTO.Common;
using Data.DTO.Product;
using Data.DTO.Product;
using Data.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Data.Repository.ProductReview
{
    public class ProductReviewRepository : IProductReviewRepository
    {
        private readonly DataContext _context;

        public ProductReviewRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> Create(CreateProductReviewDTO dto)
        {
            var check = await _context.ProductReview
                .FirstOrDefaultAsync(x =>
                    x.ProductId == dto.ProductId &&
                    x.OrderId == dto.OrderId &&
                    !x.IsDeleted);

            if (check != null)
            {
                return false;
            }

            ProductReviewEntity entity = new ProductReviewEntity()
            {
                ProductId = dto.ProductId,
                UserId = dto.UserId,
                OrderId = dto.OrderId,
                Rating = dto.Rating,
                Comment = dto.Comment,

                IsApproved = true,
                IsDeleted = false,

                CreatedBy = dto.UserId,
                CreatedDate = DateTime.Now
            };

            _context.ProductReview.Add(entity);

            await _context.SaveChangesAsync();

            if (dto.ImagePaths != null && dto.ImagePaths.Any())
            {
                foreach (var path in dto.ImagePaths)
                {
                    AttachmentEntity attachment = new AttachmentEntity()
                    {
                        EntityId = entity.Id,
                        EntityType = "ProductReview",

                        FilePath = path,
                        FileName = Path.GetFileName(path),

                        IsDeleted = false,

                        CreatedDate = DateTime.Now
                    };

                    _context.Attachment.Add(attachment);
                }

                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> Reply(CreateProductReviewReplyDTO dto)
        {
            ProductReviewReplyEntity entity = new ProductReviewReplyEntity()
            {
                ProductReviewId = dto.ProductReviewId,
                UserId = dto.UserId,
                Content = dto.Content,

                IsDeleted = false,

                CreatedBy = dto.UserId,
                CreatedDate = DateTime.Now
            };

            _context.ProductReviewReply.Add(entity);

            await _context.SaveChangesAsync();

            return true;
        }

        public List<ProductReviewListDTO> GetByProduct(int productId)
        {
            return _context.ProductReview
                .Include(x => x.User)
                .Include(x => x.Replies)
                    .ThenInclude(x => x.User)
                .Where(x =>
                    x.ProductId == productId &&
                    !x.IsDeleted &&
                    x.IsApproved == true)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new ProductReviewListDTO
                {
                    Id = x.Id,

                    ProductId = x.ProductId,

                    UserId = x.UserId,

                    UserName = x.User.UserName,

                    Rating = x.Rating,

                    Comment = x.Comment,

                    CreatedDate = x.CreatedDate,

                    Images = _context.Attachment
                        .Where(a =>
                            a.EntityId == x.Id &&
                            a.EntityType == "ProductReview" &&
                            a.IsDeleted != true)
                        .Select(a => a.FilePath)
                        .ToList(),

                    Replies = x.Replies
                        .Where(r => !r.IsDeleted)
                        .OrderBy(r => r.CreatedDate)
                        .Select(r => new ProductReviewReplyListDTO
                        {
                            Id = r.Id,

                            ProductReviewId = r.ProductReviewId,

                            UserId = r.UserId,

                            UserName = r.User.UserName,

                            Content = r.Content,

                            CreatedDate = r.CreatedDate

                        }).ToList()
                })
                .ToList();
        }

        public List<ProductReviewReplyListDTO> GetReply(int reviewId)
        {
            return _context.ProductReviewReply
                .Where(x =>
                    x.ProductReviewId == reviewId &&
                    !x.IsDeleted)
                .OrderBy(x => x.CreatedDate)
                .Select(x => new ProductReviewReplyListDTO
                {
                    Id = x.Id,
                    ProductReviewId = x.ProductReviewId,
                    UserId = x.UserId,
                    Content = x.Content,
                    CreatedDate = x.CreatedDate
                })
                .ToList();
        }

        public double GetAverageRating(int productId)
        {
            return _context.ProductReview
                .Where(x =>
                    x.ProductId == productId &&
                    !x.IsDeleted &&
                    x.IsApproved)
                .Average(x => (double?)x.Rating) ?? 0;
        }

        public List<ProductReviewStatisticDTO> GetStatistic(int productId)
        {
            return _context.ProductReview
                .Where(x =>
                    x.ProductId == productId &&
                    !x.IsDeleted &&
                    x.IsApproved)
                .GroupBy(x => x.Rating)
                .Select(x => new ProductReviewStatisticDTO
                {
                    Star = x.Key,
                    Count = x.Count()
                })
                .OrderByDescending(x => x.Star)
                .ToList();
        }

        public List<ProductReviewListDTO> GetAll()
        {
            return _context.ProductReview
                .Include(x => x.User)
                .Include(x => x.Replies)
                    .ThenInclude(x => x.User)
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsApproved)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new ProductReviewListDTO
                {
                    Id = x.Id,

                    ProductId = x.ProductId,

                    UserId = x.UserId,

                    UserName = x.User.UserName,

                    Rating = x.Rating,

                    Comment = x.Comment,

                    CreatedDate = x.CreatedDate,

                    Images = _context.Attachment
                        .Where(a =>
                            a.EntityId == x.Id &&
                            a.EntityType == "ProductReview" &&
                            a.IsDeleted != true)
                        .Select(a => a.FilePath)
                        .ToList(),

                    Replies = x.Replies
                        .Where(r => !r.IsDeleted)
                        .Select(r => new ProductReviewReplyListDTO
                        {
                            Id = r.Id,

                            ProductReviewId = r.ProductReviewId,

                            UserId = r.UserId,

                            UserName = r.User.UserName,

                            Content = r.Content,

                            CreatedDate = r.CreatedDate
                        }).ToList()
                })
                .ToList();
        }

        public PagedResult<ProductReviewProductDTO>GetProductReviewList(int page, int pageSize)
        {
            var query = _context.ProductReview
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsApproved == true)
                .GroupBy(x => new
                {
                    x.ProductId,
                    x.Product.Name
                })
                .Select(x => new ProductReviewProductDTO
                {
                    ProductId = x.Key.ProductId,

                    ProductName = x.Key.Name,

                    Thumbnail = _context.Attachment
                        .Where(a =>
                            a.EntityId == x.Key.ProductId &&
                            a.EntityType == "Product" &&
                            a.IsDeleted != true)
                        .Select(a => a.FilePath)
                        .FirstOrDefault(),

                    AverageRating = x.Average(s => s.Rating),

                    TotalReview = x.Count()
                });

            var totalItems = query.Count();

            var items = query
                .OrderByDescending(x => x.TotalReview)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<ProductReviewProductDTO>()
            {
                Items = items,

                Page = page,

                PageSize = pageSize,

                TotalItems = totalItems
            };
        }
    }
}