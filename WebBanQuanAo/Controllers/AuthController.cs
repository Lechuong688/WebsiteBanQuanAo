using Data.Entity;
using Data.Service.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebBanQuanAo.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly SignInManager<UserEntity> _signInManager;

        public AuthController(
            UserManager<UserEntity> userManager,
        SignInManager<UserEntity> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
                return View();
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                password,
                isPersistent: false,
                lockoutOnFailure: false
            );

            if (!result.Succeeded)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
                return View();
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction(
                    "Dashboardv1",
                    "Dashboard",
                    new { area = "Admin" }
                );
            }

            if (await _userManager.IsInRoleAsync(user, "User"))
            {
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(string username, string name, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword) || string.IsNullOrEmpty(name))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
                return View();
            }
            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu nhập lại không khớp";
                return View();
            }

            var user = new UserEntity
            {
                UserName = username,
                Email = email,
                Name = name,
                CreatedDate = DateTime.Now,
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = string.Join("<br/>",
    result.Errors.Select(e => e.Description));

            return View();
        }


        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Auth");

            var properties = _signInManager
                .ConfigureExternalAuthenticationProperties(
                    "Google",
                    redirectUrl);

            return Challenge(properties, "Google");
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                return RedirectToAction("Login");
            }

            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                false);

            // Nếu đã có tài khoản
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            // Lấy email từ Google
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (email == null)
            {
                return RedirectToAction("Login");
            }

            // Kiểm tra user đã tồn tại chưa
            var existingUser = await _userManager.FindByEmailAsync(email);

            UserEntity user;

            if (existingUser == null)
            {
                user = new UserEntity
                {
                    UserName = email,
                    Email = email,
                    Name = email,
                    CreatedDate = DateTime.Now
                };

                var createResult = await _userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                {
                    ViewBag.Error = "Không tạo được tài khoản";
                    return View("Login");
                }

                // Gán role User
                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                user = existingUser;
            }

            // Liên kết Google với tài khoản
            await _userManager.AddLoginAsync(user, info);

            // Đăng nhập
            await _signInManager.SignInAsync(user, false);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult FacebookLogin()
        {
            var redirectUrl = Url.Action("FacebookResponse", "Auth");

            var properties = _signInManager
                .ConfigureExternalAuthenticationProperties(
                    "Facebook",
                    redirectUrl);

            return Challenge(properties, "Facebook");
        }

        [HttpGet]
        public async Task<IActionResult> FacebookResponse()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                return RedirectToAction("Login");
            }

            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                false);

            // Nếu tài khoản đã tồn tại
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            // Lấy email từ Facebook
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);

            // Facebook đôi khi không trả email
            if (email == null)
            {
                email = Guid.NewGuid().ToString() + "@facebook.com";
            }

            // Kiểm tra user đã tồn tại chưa
            var existingUser = await _userManager.FindByEmailAsync(email);

            UserEntity user;

            if (existingUser == null)
            {
                user = new UserEntity
                {
                    UserName = "fb_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                    Email = email,
                    Name = name ?? email,
                    CreatedDate = DateTime.Now
                };

                var createResult = await _userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                {
                    ViewBag.Error = "Không tạo được tài khoản Facebook";
                    return View("Login");
                }

                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                user = existingUser;
            }

            // Liên kết Facebook với tài khoản
            await _userManager.AddLoginAsync(user, info);

            // Đăng nhập
            await _signInManager.SignInAsync(user, false);

            return RedirectToAction("Index", "Home");
        }
    }
}
