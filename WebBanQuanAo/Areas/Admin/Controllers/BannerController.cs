using Data.DTO.Banner;
using Data.Repository;
using Data.Repository.Banner;
using Data.Repository.Collection;
using Data.Repository.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebBanQuanAo.Areas.Admin.Models;

namespace WebBanQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BannerController : Controller
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly ICollectionRepository _collectionRepository;
        private readonly DataContext _context;
        public BannerController(DataContext context, IBannerRepository bannerRepository, ICollectionRepository collectionRepository)
        {
            _context = context;
            _bannerRepository = bannerRepository;
            _collectionRepository = collectionRepository;
        }
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? keyword = null, bool? status = null)
        {
            var result = await _bannerRepository.GetList(page, pageSize, keyword, status);

            ViewBag.Keyword = keyword;
            ViewBag.Status = status;
            ViewBag.PageSize = pageSize;

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Save(int? id)
        {
            var model = new BannerUpdateDTO();

            if (id.HasValue)
            {
                var banner = await _bannerRepository.GetById(id.Value);
                if (banner == null)
                    return NotFound();

                model.Id = banner.Id;
                model.Title = banner.Title;
                model.SubTitle = banner.SubTitle;
                model.ButtonText = banner.ButtonText;
                model.Description = banner.Description;
                model.CollectionId = banner.CollectionId;
                model.DisplayOrder = banner.DisplayOrder;
                model.IsActive = banner.IsActive;

                var image = _context.Attachment
                    .FirstOrDefault(x => x.EntityId == banner.Id
                                      && x.EntityType == "Banner"
                                      && x.IsDeleted != true);

                if (image != null)
                {
                    model.ExistingImagePath = image.FilePath;
                }
            }

            ViewBag.Collections = new SelectList(
                _collectionRepository.GetAll(),
                "Id",
                "Name"
            );

            return View(model);
        }

        [HttpPost]
        public IActionResult Save(BannerUpdateDTO dto)
        {
            _bannerRepository.Save(dto);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _bannerRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
