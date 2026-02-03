using Data.DTO.Product;
using Data.Entity;
using Data.Repository.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebBanQuanAo.Models;

namespace WebBanQuanAo.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly IProductRepository _productRepository;
        public HomeController(SignInManager<UserEntity> signInManager, ILogger<HomeController> logger, IProductRepository productRepository)
        {
            _logger = logger;
            _signInManager = signInManager;
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index()
        {
            //var products = _productRepository.GetAll();
            //return View(products);
            var products = await _productRepository.GetList(1, 20);
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
