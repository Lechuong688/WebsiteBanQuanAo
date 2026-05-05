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
                .Where(x =>
                    x.ProductId == productId &&
                    !x.IsDeleted &&
                    x.IsApproved)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new ProductReviewListDTO
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    UserId = x.UserId,
                    Rating = x.Rating,
                    Comment = x.Comment,
                    CreatedDate = x.CreatedDate
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
    }
}