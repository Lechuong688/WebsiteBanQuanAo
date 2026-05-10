using Data.DTO.User;
using Data.Entity;
using Data.Repository;
using Data.Repository.Attachment;
using Data.Repository.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebBanQuanAo.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<UserEntity> _userManager;
        private readonly DataContext _context;

        public ProfileController(IUserRepository userRepository, UserManager<UserEntity> userManager, DataContext context)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var model = await _userRepository
                .GetProfileAsync(userId);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(
    ProfileDTO model,
    IFormFile? avatarFile)
        {
            if (avatarFile != null)
            {
                var fileName =
                    Guid.NewGuid().ToString()
                    + Path.GetExtension(avatarFile.FileName);

                var folder =
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot/uploads/avatar");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var path = Path.Combine(folder, fileName);

                using (var stream =
                       new FileStream(path, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }

                var attachment = new AttachmentEntity
                {
                    FileName = fileName,
                    FilePath = "/uploads/avatar/" + fileName,
                    CreatedDate = DateTime.Now
                };

                _context.Attachment.Add(attachment);

                await _context.SaveChangesAsync();

                model.AvatarId = attachment.Id;
            }

            await _userRepository.UpdateProfileAsync(model);

            TempData["success"] =
                "Cập nhật thông tin thành công";

            return RedirectToAction("Index");
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordDTO model)
        {
            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["error"] =
                    "Xác nhận mật khẩu không khớp";

                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            var result = await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword);

            if (!result.Succeeded)
            {
                TempData["error"] =
                    result.Errors.First().Description;

                return View(model);
            }

            TempData["success"] =
                "Đổi mật khẩu thành công";

            return RedirectToAction("Index");
        }
    }
}