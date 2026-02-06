using Data.Entity.Enums;
using Data.Repository.MasterData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebBanQuanAo.Areas.Admin.Models;

namespace WebBanQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class CategoryController : Controller
    {
        private readonly IMasterDataRepository _masterDataRepository;

        public CategoryController(IMasterDataRepository masterDataRepository)
        {
            _masterDataRepository = masterDataRepository;
        }

        public IActionResult Index()
        {
            var categories = _masterDataRepository.GetCategories();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Save(int? id)
        {
            if (id == null)
            {
                return View(new MasterDataViewModel
                {
                    Type = MasterDataType.Category,
                    ParentItems = _masterDataRepository.GetMainCategories()
                });

            }

            var category = _masterDataRepository.GetCategoryById(id.Value);
            if (category == null)
                return NotFound();

            return View(new MasterDataViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Code = category.Code,
                Note = category.Note,
                ParentId = category.GroupId,
                Type = MasterDataType.Category,
                ParentItems = _masterDataRepository.GetMainCategories()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(MasterDataViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!ModelState.IsValid)
            {
                model.Type = MasterDataType.Category;
                model.ParentItems = _masterDataRepository.GetMainCategories();
                return View(model);
            }

            if (model.Id == 0)
            {
                _masterDataRepository.CreateCategory(model.Name, model.Code, model.Note, model.ParentId, userId);
            }
            else
            {
                _masterDataRepository.UpdateCategory(
                    model.Id,
                    model.Name,
                    model.Code,
                    model.Note,
                    model.ParentId,
                    userId
                );
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            _masterDataRepository.DeleteCategory(id);
            return RedirectToAction("Index");
        }
    }
}
