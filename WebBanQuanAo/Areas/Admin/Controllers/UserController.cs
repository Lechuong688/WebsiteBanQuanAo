using Data.Entity;
using Data.Repository.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net;
using System.Threading.Tasks;
using WebBanQuanAo.Areas.Admin.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebBanQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class UserController : Controller
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly RoleManager<RoleEntity> _roleManager;
        private readonly IUserRepository _userRepository;
        public UserController(UserManager<UserEntity> userManager, 
            RoleManager<RoleEntity> roleManager, IUserRepository userRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userRepository = userRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> LoadUsers(string? keyword, int page = 1, int pageSize = 5)
        {

            var result = await _userRepository.GetUserAsync(keyword, page, pageSize);
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPage =
                (int)Math.Ceiling(result.Total / (double)pageSize);
            ViewBag.Keyword = keyword;
            ViewBag.UserRoles = await _userRepository.GetUserRolesAsync(result.Users);

            return PartialView("_UserTable", result.Users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLock(string id)
        {
            try
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

                var lockAdmin = _userManager.GetUserId(User);
                if (user.Id == lockAdmin)
                {
                    return BadRequest("Không thể tự khóa chính mình");
                }

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return StatusCode(500);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("Có lỗi xảy ra" + ex.Message);
            }
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel createmodel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(createmodel);
                }

                var checkUserCreate = await _userManager.FindByNameAsync(createmodel.UserName);
                if (checkUserCreate != null)
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại");
                    return View(createmodel);
                }
                var currentUser = await _userManager.GetUserAsync(User);

                var user = new UserEntity
                {
                    UserName = createmodel.UserName,
                    Email = createmodel.Email,
                    Name = createmodel.Name,
                    PhoneNumber = createmodel.PhoneNumber,
                    CreatedDate = DateTime.Now
                    //EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, createmodel.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);

                    return View(createmodel);
                }

                await _userManager.AddToRoleAsync(user, "User");

                TempData["SuccessMessage"] = "Thêm người dùng thành công";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest("Có lỗi xảy ra" + ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> UpdateUser(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return NotFound();

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return Content("Không tìm thấy user");

                var model = new UpdateUserViewModel
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    SelectedRoles = (await _userManager.GetRolesAsync(user)).ToList(),
                    AllRoles = _roleManager.Roles.Select(r => r.Name).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return BadRequest("Có lỗi xảy ra" + ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(UpdateUserViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                    return View(model);
                }

                var checkUserUpdate = await _userManager.FindByNameAsync(model.UserName);
                if (checkUserUpdate != null && checkUserUpdate.Id != model.Id)
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại");
                    model.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                    return View(model);
                }

                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                    return NotFound();

                user.Name = model.Name;
                user.Email = model.Email;
                user.UserName = model.UserName;
                user.PhoneNumber = model.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);

                    model.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                    return View(model);
                }
                var currentRoles = await _userManager.GetRolesAsync(user);
                var selectedRoles = model.SelectedRoles ?? new List<string>();

                var rolesToAdd = selectedRoles.Except(currentRoles).ToList();
                var rolesToRemove = currentRoles.Except(selectedRoles).ToList();

                if (rolesToAdd.Any())
                    await _userManager.AddToRolesAsync(user, rolesToAdd);

                if (rolesToRemove.Any())
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

                TempData["Success"] = "Cập nhật người dùng thành công";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest("Có lỗi xảy ra: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return Content("Không tìm thấy user");

                var currentUserId = _userManager.GetUserId(User);
                if (user.Id == currentUserId)
                {
                    return BadRequest("Không thể tự xóa chính mình");
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return StatusCode(500);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("Có lỗi xảy ra" + ex.Message);
            }
        }
    }
}
