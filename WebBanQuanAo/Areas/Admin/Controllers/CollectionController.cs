using Data.Entity;
using Data.Repository.Collection;
using Data.Repository.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebBanQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class CollectionController : Controller
    {
        private readonly ICollectionRepository _collectionRepository;
        private readonly IProductRepository _productRepository;

        public CollectionController(ICollectionRepository collectionRepository, IProductRepository productRepository)
        {
            _collectionRepository = collectionRepository;
            _productRepository = productRepository;
        }
        public IActionResult Index()
        {
            var data = _collectionRepository.GetAll();
            return View(data);
        }

        public IActionResult Save(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new CollectionEntity());
            }

            var collection = _collectionRepository.GetById(id.Value);
            if (collection == null)
                return NotFound();

            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(CollectionEntity entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Code))
            {
                entity.Code = _collectionRepository.GenerateCode(entity.Name);
                ModelState.Remove("Code");
            }

            if (_collectionRepository.IsCodeExists(entity.Code, entity.Id))
            {
                ModelState.AddModelError("Code", "Code đã tồn tại, vui lòng nhập code khác");
                return View(entity);
            }

            var isCreate = entity.Id == 0;

            if (!ModelState.IsValid)
            {
                return View(entity);
            }

            _collectionRepository.Save(entity);
            if (isCreate)
            {
                return RedirectToAction("Product", new { id = entity.Id });
            }
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public IActionResult Delete(int id)
        {
            _collectionRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Product(int id)
        {
            var collection = _collectionRepository.GetById(id);
            if (collection == null)
                return NotFound();

            var products = _productRepository.GetAll().ToList();

            ViewBag.Products = products;
            ViewBag.SelectedProductIds = _collectionRepository.GetProductIds(id);

            return View(collection);
        }

        [HttpPost]
        public IActionResult Product(int collectionId, List<int> productIds)
        {
            _collectionRepository.SaveProducts(collectionId, productIds);
            return RedirectToAction(nameof(Index));
        }

    }
}
