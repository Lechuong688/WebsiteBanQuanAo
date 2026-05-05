using Data.DTO.Product;
using Data.Repository.ProductReview;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebBanQuanAo.Controllers
{
    public class ProductReviewController : Controller
    {
        private readonly IProductReviewRepository _productReviewRepository;

        public ProductReviewController(
            IProductReviewRepository productReviewRepository)
        {
            _productReviewRepository = productReviewRepository;
        }

        // =========================
        // TẠO REVIEW
        // =========================

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductReviewDTO dto)
        {
            try
            {
                dto.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var result = await _productReviewRepository.Create(dto);

                if (!result)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Bạn đã đánh giá sản phẩm này rồi"
                    });
                }

                return Json(new
                {
                    success = true,
                    message = "Đánh giá thành công"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // =========================
        // LẤY REVIEW THEO PRODUCT
        // =========================

        [HttpGet]
        public IActionResult GetByProduct(int productId)
        {
            var data = _productReviewRepository
                .GetByProduct(productId);

            return Json(data);
        }

        // =========================
        // REPLY REVIEW
        // =========================

        [HttpPost]
        public async Task<IActionResult> Reply(CreateProductReviewReplyDTO dto)
        {
            await _productReviewRepository.Reply(dto);

            return Json(new
            {
                success = true,
                message = "Phản hồi thành công"
            });
        }

        // =========================
        // LẤY REPLY
        // =========================

        [HttpGet]
        public IActionResult GetReply(int reviewId)
        {
            var data = _productReviewRepository
                .GetReply(reviewId);

            return Json(data);
        }

        // =========================
        // SAO TRUNG BÌNH
        // =========================

        [HttpGet]
        public IActionResult GetAverageRating(int productId)
        {
            var data = _productReviewRepository
                .GetAverageRating(productId);

            return Json(data);
        }

        // =========================
        // THỐNG KÊ SỐ SAO
        // =========================

        [HttpGet]
        public IActionResult GetStatistic(int productId)
        {
            var data = _productReviewRepository
                .GetStatistic(productId);

            return Json(data);
        }

        [HttpPost]
        public async Task<IActionResult> UploadImages(
    List<IFormFile> files)
        {
            List<string> paths = new List<string>();

            string folder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/uploads/UploadReviewImages");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            foreach (var file in files)
            {
                string fileName =
                    Guid.NewGuid() +
                    Path.GetExtension(file.FileName);

                string fullPath =
                    Path.Combine(folder, fileName);

                using (var stream = new FileStream(
                    fullPath,
                    FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                paths.Add("/uploads/UploadReviewImages/" + fileName);
            }

            return new JsonResult(paths);
        }
    }
}