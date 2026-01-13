using Data.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Threading.Tasks;
using WebBanQuanAo.Areas.Admin.Models;

namespace WebBanQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class UserController : Controller
    {
        private readonly UserManager<UserEntity> _userManager;
        public UserController(UserManager<UserEntity> userManager)
        {
            _userManager = userManager;
        }
        
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> LoadUsers(string? keyword, int page = 1, int pageSize = 5)
        {
            if (page < 1)
                page = 1;
            if (pageSize <= 0) pageSize = 5;

            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    x.UserName.Contains(keyword) ||
                    x.Email.Contains(keyword) ||
                    x.Name.Contains(keyword)
                );
            }

            var totalUsers = query.Count();

            var users = query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            //var userRoles = new Dictionary<string, string>();

            //foreach (var item in users)
            //{
            //    var roles = await _userManager.GetRolesAsync(item);
            //    userRoles[item.Id] = roles.FirstOrDefault() ?? "User";
            //}

            //ViewBag.UserRoles = userRoles;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPage =
                (int)Math.Ceiling(totalUsers / (double)pageSize);
            ViewBag.Keyword = keyword;

            return PartialView("_UserTable", users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLock(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            if (!user.LockoutEnabled)
            {
                user.LockoutEnabled = true;
            }

            var isLocked =
                user.LockoutEnd.HasValue &&
                user.LockoutEnd.Value > DateTimeOffset.Now;

            if (isLocked)
            {
                user.LockoutEnd = null;
            }
            else
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return StatusCode(500);

            return Ok();
        }


        [HttpGet]
        public async Task<IActionResult> UpdateUser(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            Console.WriteLine("ID = " + id);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Content("KHÔNG TÌM THẤY USER");

            var model = new UpdateUserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(UpdateUserViewModel model)
        {
            ModelState.Clear();

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ";
                return RedirectToAction("UpdateUser", new { id = model.Id });
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            user.Name = model.Name;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            return RedirectToAction("Index");
        }
    }
}
