using Data.DTO.Product;
using Data.Repository.Collection;
using Data.Repository.Product;
using Microsoft.AspNetCore.Mvc;

namespace WebBanQuanAo.Controllers
{
    public class CollectionController : Controller
    {
        private readonly ICollectionRepository _collectionRepository;
        private readonly IProductRepository _productRepository;
        public CollectionController(ICollectionRepository collectionRepository, IProductRepository productRepository)
        {
            _collectionRepository = collectionRepository;
            _productRepository = productRepository;
        }
        public IActionResult Index(string code)
        {
            ViewBag.Categories = _productRepository.GetCategories();
            ViewBag.Colors = _productRepository.GetColors();

            var data = _collectionRepository.GetCollectionForUser(code);
            if (data == null)
                return NotFound();

            ViewBag.Collection = data.Value.collection;

            var products = data.Value.products;

            var model = new PagedResult<ProductListDTO>
            {
                Items = products,
                Page = 1,
                PageSize = products.Count == 0 ? 1 : products.Count,
                TotalItems = products.Count
            };

            return View(model);
        }


        public IActionResult LoadProducts(string collectionCode, int page = 1, int pageSize = 8, int? typeId = null,
            List<int> colorIds = null, decimal? maxPrice = null, string keyword = "", string sort = "")
        {
            var result = _productRepository.GetForCollectionPaged(
                collectionCode,
                page,
                pageSize,
                typeId,
                colorIds,
                maxPrice,
                keyword,
                sort
            );

            return PartialView("_ProductCollectionItems", result);
        }

    }
}
