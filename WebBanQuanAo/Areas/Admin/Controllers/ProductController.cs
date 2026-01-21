using Data.DTO;
using Data.Repository.MasterData;
using Data.Repository.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBanQuanAo.Areas.Admin.Models;

namespace WebBanQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IMasterDataRepository _masterDataRepository;

        public ProductController(IProductRepository productRepository, IMasterDataRepository masterDataRepository)
        {
            _productRepository = productRepository;
            _masterDataRepository = masterDataRepository;
        }
        public IActionResult Index()
        {
            var products = _productRepository.GetAll();
            return View(products);
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
                    Sizes = _masterDataRepository.GetSizes()
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
                    Sizes = _masterDataRepository.GetSizes()
                };
            }

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(UpdateProductViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Colors = _masterDataRepository.GetColors();
                vm.Sizes = _masterDataRepository.GetSizes();
                return View(vm);
            }

            var imagePaths = new List<string>();

            var dto = new ProductUpdateDTO
            {
                Id = vm.Id,
                Name = vm.Name,
                Price = vm.Price,
                Quantity = vm.Quantity,
                ColorIds = vm.ColorIds,
                SizeIds = vm.SizeIds,
                ImagePaths = imagePaths,
                DeletedImageIds = vm.DeletedImageIds
            };

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

                _productRepository.Save(dto);

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
