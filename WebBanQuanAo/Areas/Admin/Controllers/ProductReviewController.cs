using Data.DTO.Product;
using Data.Repository.ProductReview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebBanQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductReviewController : Controller
    {
        private readonly IProductReviewRepository
            _productReviewRepository;

        public ProductReviewController(
            IProductReviewRepository productReviewRepository)
        {
            _productReviewRepository =
                productReviewRepository;
        }

        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            var data = _productReviewRepository.GetProductReviewList(page, pageSize);

            return View(data);
        }


        public IActionResult Detail(int productId)
        {
            var data = _productReviewRepository
                .GetByProduct(productId);

            ViewBag.ProductId = productId;

            return View(data);
        }


        [HttpPost]
        public async Task<IActionResult> Reply(
            CreateProductReviewReplyDTO dto,
            int productId)
        {
            dto.UserId = User
                .FindFirst(ClaimTypes.NameIdentifier)
                ?.Value;

            await _productReviewRepository
                .Reply(dto);

            TempData["success"] =
                "Phản hồi thành công";

            return RedirectToAction(
                "Detail",
                new
                {
                    productId = productId
                });
        }
    }
}