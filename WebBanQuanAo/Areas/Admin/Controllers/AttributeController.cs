using Data.Repository.MasterData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBanQuanAo.Areas.Admin.Models;
using Data.Entity.Enums;

namespace WebBanQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class AttributeController : Controller
    {
        private readonly IMasterDataRepository _masterDataRepository;

        public AttributeController(IMasterDataRepository masterDataRepository)
        {
            _masterDataRepository = masterDataRepository;
        }

        public IActionResult Index()
        {
            var attributes = _masterDataRepository.GetAttributes();
            return View(attributes);
        }

        [HttpGet]
        public IActionResult Save(int? id)
        {
            if (id == null)
            {
                return View(new MasterDataViewModel
                {
                    Type = MasterDataType.Attribute,
                    ParentItems = _masterDataRepository.GetMainAttributes()
                });
            }

            // SỬA
            var attribute = _masterDataRepository.GetAttributeById(id.Value);
            if (attribute == null)
                return NotFound();

            return View(new MasterDataViewModel
            {
                Id = attribute.Id,
                Name = attribute.Name,
                Code = attribute.Code,
                Note = attribute.Note,
                ParentId = attribute.GroupId,
                Type = MasterDataType.Attribute,
                ParentItems = _masterDataRepository.GetMainAttributes()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(MasterDataViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Type = MasterDataType.Attribute;
                model.ParentItems = _masterDataRepository.GetMainAttributes();
                return View(model);
            }

            if (model.Id == 0)
            {
                _masterDataRepository.CreateAttribute(
                    model.Name,
                    model.Code,
                    model.Note,
                    model.ParentId
                );
            }
            else
            {
                _masterDataRepository.UpdateAttribute(
                    model.Id,
                    model.Name,
                    model.Code,
                    model.Note,
                    model.ParentId
                );
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            _masterDataRepository.DeleteAttribute(id);
            return RedirectToAction("Index");
        }
    }
}
