using Data.Entity;
using Data.Repository.Discount;
using Data.Repository.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebBanQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DiscountCodeController : Controller
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly IProductRepository _productRepository;

        public DiscountCodeController(IDiscountRepository discountRepository, IProductRepository productRepository)
        {
            _discountRepository = discountRepository;
            _productRepository = productRepository;
        }
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var result = await _discountRepository.GetListDiscountCode(page, pageSize);

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            return View(result);
        }

        public IActionResult Save(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new DiscountCodeEntity());
            }

            var discountCode = _discountRepository.GetByIdDiscountCode(id.Value);
            if (discountCode == null)
            {
                return NotFound();
            }
            return View(discountCode);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(DiscountCodeEntity discountCode)
        {
            var isCreate = discountCode.Id == 0;

            if (!ModelState.IsValid)
            {
                return View(discountCode);
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (isCreate)
            {
                discountCode.CreatedBy = userId;
            }
            else
            {
                discountCode.UpdatedBy = userId;
            }

            _discountRepository.SaveDiscountCode(discountCode);
            if (isCreate)
            {
                return RedirectToAction("Product", new { id = discountCode.Id });
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            _discountRepository.DeleteDiscountCode(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
