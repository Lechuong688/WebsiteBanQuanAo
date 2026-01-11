using Data.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebBanQuanAo.Areas.Admin.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<UserEntity> _userManager;
        public UserController(UserManager<UserEntity> userManager)
        {
            _userManager = userManager;
        }
        [Area("Admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }
    }
}
