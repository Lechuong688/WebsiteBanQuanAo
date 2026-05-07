using Data.DTO.Product;
using Data.Entity;
using Data.Repository.Banner;
using Data.Repository.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Drawing;
using System.Security.Claims;
using WebBanQuanAo.Models;

namespace WebBanQuanAo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly IProductRepository _productRepository;
        private readonly IBannerRepository _bannerRepository;

        public HomeController(
            SignInManager<UserEntity> signInManager,
            ILogger<HomeController> logger,
            IProductRepository productRepository,
            IBannerRepository bannerRepository)
        {
            _logger = logger;
            _signInManager = signInManager;
            _productRepository = productRepository;
            _bannerRepository = bannerRepository;
        }
        public async Task<IActionResult> Index(string keyword = null, string size = null, string color = null, decimal? minPrice = null, decimal? maxPrice = null, int page = 1, int pageSize = 8)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var products = await _productRepository.GetList(userId, keyword, size, color, minPrice, maxPrice, page, pageSize);
            var bannerResult = await _bannerRepository.GetList(1, 10, null, true);
            var pinned = await _productRepository.GetPinned();
            var bestSeller = await _productRepository.GetBestseller();
            var topSelling = await _productRepository.GetTopSelling();
            var newArrival = await _productRepository.GetNewArrival(userId);
            //var productWishlist = await _productRepository.GetProductWishlist(1, 8);

            ViewBag.Banners = bannerResult.Items;
            ViewBag.PinnedProduct = pinned;
            ViewBag.BestSeller = bestSeller;
            ViewBag.TopSelling = topSelling;
            ViewBag.NewArrival = newArrival;
            //ViewBag.ProductWishlist = productWishlist;

            ViewBag.Keyword = keyword;
            ViewBag.Size = size;
            ViewBag.Color = color;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            return View(products?.Items ?? new List<ProductListDTO>());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
