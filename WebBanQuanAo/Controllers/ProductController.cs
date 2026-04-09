using Data.DTO.Product;
using Data.Entity;
using Data.Repository.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebBanQuanAo.Controllers
{
    public class ProductController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly IProductRepository _productRepository;

        public ProductController(SignInManager<UserEntity> signInManager, ILogger<HomeController> logger, IProductRepository productRepository)
        {
            _logger = logger;
            _signInManager = signInManager;
            _productRepository = productRepository;
        }
        public IActionResult Index()
        {
            ViewBag.Categories = _productRepository.GetCategories();
            ViewBag.Colors = _productRepository.GetColors();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var products = _productRepository.GetAll(userId);
            return View(products);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var product = await _productRepository.GetDetail(id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        public IActionResult LoadProducts(
        int page = 1,
        int pageSize = 8,
        int? typeId = null,
        List<int> colorIds = null,
        decimal? maxPrice = null,
        string keyword = "",
        string sort = "")
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = _productRepository.GetForShopPaged(
                page,
                pageSize,
                typeId,
                colorIds,
                maxPrice,
                keyword,
                sort,
                userId
            );

            System.Diagnostics.Debug.WriteLine(
            colorIds == null
        ? "COLOR IDS = NULL"
        : "COLOR IDS = " + string.Join(",", colorIds)
        );

            return PartialView("_ProductItems", result);
        }

        [Authorize]
        public async Task<IActionResult> ProductWishlist(int page = 1, int pageSize = 8)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var productWishlist = await _productRepository.GetProductWishlist(userId, page, pageSize);
            ViewBag.ProductWishlist = productWishlist;
            return View(productWishlist?.Items ?? new List<ProductWishlistDTO>());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ToggleWishlist([FromBody] int productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var isAdded = await _productRepository.ToggleWishlist(productId, userId);

            return Json(new { isAdded });
        }

    }
}
