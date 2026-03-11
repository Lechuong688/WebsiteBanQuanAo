using Data.DTO.Product;
using Data.Repository.MasterData;
using Data.Repository.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using WebBanQuanAo.Areas.Admin.Models;

namespace WebBanQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IMasterDataRepository _masterDataRepository;

        public ProductController(IProductRepository productRepository, IMasterDataRepository masterDataRepository)
        {
            _productRepository = productRepository;
            _masterDataRepository = masterDataRepository;
        }
        
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var result = await _productRepository.GetList(page, pageSize);

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            return View(result);
        }

        [HttpGet]
        public IActionResult Save(int? id)
        {
            UpdateProductViewModel vm;

            if (id == null || id == 0)
            {
                vm = new UpdateProductViewModel
                {
                    Id = 0,
                    Colors = _masterDataRepository.GetColors(),
                    Sizes = _masterDataRepository.GetSizes(),
                    Types = _masterDataRepository.GetProductTypes()
                };
            }
            else
            {
                var dto = _productRepository.GetById(id.Value);
                if (dto == null) return NotFound();

                vm = new UpdateProductViewModel
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Price = dto.Price,
                    Quantity = dto.Quantity,
                    Note = dto.Note,
                    TypeId = dto.TypeId,
                    IsPinned = dto.IsPinned,
                    ColorIds = dto.ColorIds,
                    SizeIds = dto.SizeIds,

                    oldImages = _productRepository
                        .GetImagesByProductId(dto.Id)
                        .Select(a => new ImageVM
                        {
                            Id = a.Id,
                            Path = a.FilePath
                        }).ToList(),

                    Colors = _masterDataRepository.GetColors(),
                    Sizes = _masterDataRepository.GetSizes(),
                    Types = _masterDataRepository.GetProductTypes()
                };
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(UpdateProductViewModel vm)
        {
            //ModelState.Remove("IsPinned");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!ModelState.IsValid)
            {
                vm.Colors = _masterDataRepository.GetColors();
                vm.Sizes = _masterDataRepository.GetSizes();
                vm.Types = _masterDataRepository.GetProductTypes();
                return View(vm);
            }

            var imagePaths = new List<string>();

            var dto = new ProductUpdateDTO
            {
                Id = vm.Id,
                Name = vm.Name,
                Price = vm.Price,
                Quantity = vm.Quantity,
                Note = vm.Note,
                TypeId = vm.TypeId,
                IsPinned = vm.IsPinned,
                ColorIds = vm.ColorIds,
                SizeIds = vm.SizeIds,
                ImagePaths = imagePaths,
                DeletedImageIds = vm.DeletedImageIds,
                UserId = userId
            };

            if (vm.Price <= 0)
            {
                ModelState.AddModelError(nameof(vm.Price), "Giá sản phẩm phải lớn hơn 0");

                vm.Colors = _masterDataRepository.GetColors();
                vm.Sizes = _masterDataRepository.GetSizes();
                vm.Types = _masterDataRepository.GetProductTypes();

                return View(vm);
            }
            try
            {
                if (vm.newImages != null)
                {
                    foreach (var file in vm.newImages)
                    {
                        if (file == null || file.Length == 0)
                            continue;

                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        var physicalPath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot/uploads/products",
                            fileName
                        );

                        using var stream = new FileStream(physicalPath, FileMode.Create);
                        file.CopyTo(stream);

                        imagePaths.Add("/uploads/products/" + fileName);
                    }
                }

                var product = _productRepository.Save(dto);

                if (vm.IsPinned)
                {
                    await _productRepository.SetPinned(product.Id);
                }

                TempData["Success"] = vm.Id > 0
                    ? "Cập nhật sản phẩm thành công"
                    : "Thêm sản phẩm thành công";

                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "Lưu sản phẩm thất bại";
                return View(vm);
            }
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var product = _productRepository.GetForDelete(id);
            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _productRepository.Delete(id);
                TempData["Success"] = "Xóa sản phẩm thành công";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
