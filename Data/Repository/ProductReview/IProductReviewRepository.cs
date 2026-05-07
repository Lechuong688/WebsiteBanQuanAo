using Data.DTO.Common;
using Data.DTO.Product;
using Data.DTO.Product;

namespace Data.Repository.ProductReview
{
    public interface IProductReviewRepository
    {
        Task<bool> Create(CreateProductReviewDTO dto);

        Task<bool> Reply(CreateProductReviewReplyDTO dto);

        List<ProductReviewListDTO> GetByProduct(int productId);

        List<ProductReviewReplyListDTO> GetReply(int reviewId);

        double GetAverageRating(int productId);

        List<ProductReviewStatisticDTO> GetStatistic(int productId);
        List<ProductReviewListDTO> GetAll();
        PagedResult<ProductReviewProductDTO> GetProductReviewList(int page, int pageSize);
    }
}