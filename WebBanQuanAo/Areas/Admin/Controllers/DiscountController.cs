using Data.Entity;
using Data.Repository.Discount;
using Data.Repository.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebBanQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DiscountController : Controller
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly IProductRepository _productRepository;
        public DiscountController(IDiscountRepository discountRepository, IProductRepository productRepository)
        {
            _discountRepository = discountRepository;
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var result = await _discountRepository.GetList(page, pageSize);

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            return View(result);
        }

        public IActionResult Save(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new DiscountEntity());
            }

            var discount = _discountRepository.GetById(id.Value);
            if (discount == null)
                return NotFound();

            return View(discount);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(DiscountEntity entity)
        {

            var isCreate = entity.Id == 0;

            if (!ModelState.IsValid)
            {
                return View(entity);
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (isCreate)
            {
                entity.CreatedBy = userId;
            }
            else
            {
                entity.UpdatedBy = userId;
            }

            _discountRepository.Save(entity);
            if (isCreate)
            {
                return RedirectToAction("Product", new { id = entity.Id });
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            _discountRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Product(int id)
        {
            var discount = _discountRepository.GetById(id);
            if (discount == null)
                return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var products = _productRepository.GetAll(userId).ToList();

            ViewBag.Products = products;
            ViewBag.SelectedProductIds = _discountRepository.GetProductIds(id);

            return View(discount);
        }

        [HttpPost]
        public IActionResult Product(int discountId, List<int> productIds)
        {
            _discountRepository.SaveProducts(discountId, productIds);
            return RedirectToAction(nameof(Index));
        }
    }
}