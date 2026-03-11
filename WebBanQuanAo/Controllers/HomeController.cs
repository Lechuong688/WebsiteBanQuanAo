using Data.DTO.Product;
using Data.Entity;
using Data.Repository.Banner;
using Data.Repository.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
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
        public async Task<IActionResult> Index()
        {
            //var products = _productRepository.GetAll();
            //return View(products);
            var products = await _productRepository.GetList(1, 20);
            var bannerResult = await _bannerRepository.GetList(1, 10, null, true);
            var pinned = await _productRepository.GetPinned();
            var bestSeller = await _productRepository.GetBestseller();

            ViewBag.Banners = bannerResult.Items;
            ViewBag.PinnedProduct = pinned;
            ViewBag.BestSeller = bestSeller;
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
