using Data.Entity;
using Data.Repository.Product;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
            var products = _productRepository.GetAll();
            return View(products);
        }

        public IActionResult Detail()
        {
            return View();
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
            var result = _productRepository.GetForShopPaged(
                page,
                pageSize,
                typeId,
                colorIds,
                maxPrice,
                keyword,
                sort
            );

            return PartialView("_ProductItems", result);
        }

    }
}
