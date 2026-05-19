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
        private readonly IEmailService _emailService;

        public AuthController(
            UserManager<UserEntity> userManager,
        SignInManager<UserEntity> signInManager,
        IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
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
        [HttpPost]
        public async Task<IActionResult> Register(string username, string name, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(username)
                || string.IsNullOrEmpty(email)
                || string.IsNullOrEmpty(password)
                || string.IsNullOrEmpty(confirmPassword)
                || string.IsNullOrEmpty(name))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu nhập lại không khớp";
                return View();
            }

            var existingEmail =
                await _userManager.FindByEmailAsync(email);

            if (existingEmail != null)
            {
                ViewBag.Error = "Email đã tồn tại";
                return View();
            }

            var otp =
                new Random().Next(100000, 999999).ToString();

            HttpContext.Session.SetString("OTP", otp);

            HttpContext.Session.SetString(
                "RegisterUsername", username);

            HttpContext.Session.SetString(
                "RegisterName", name);

            HttpContext.Session.SetString(
                "RegisterEmail", email);

            HttpContext.Session.SetString(
                "RegisterPassword", password);

            await _emailService.SendOtpEmail(email, otp);

            return RedirectToAction("VerifyOtp");
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

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (email == null)
            {
                return RedirectToAction("Login");
            }

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

                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                user = existingUser;
            }

            await _userManager.AddLoginAsync(user, info);

            await _signInManager.SignInAsync(user, false);
            await _emailService.SendWelcomeEmail( user.Email, user.Name);

            TempData["Success"] = $"🎉 Chào mừng {user.Name} đến với VYBE!";

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

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);

            if (email == null)
            {
                email = Guid.NewGuid().ToString() + "@facebook.com";
            }

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

            await _userManager.AddLoginAsync(user, info);

            await _signInManager.SignInAsync(user, false);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string otp)
        {
            var sessionOtp = HttpContext.Session.GetString("OTP");

            if (otp != sessionOtp)
            {
                ViewBag.Error = "OTP không đúng";
                return View();
            }

            var user = new UserEntity
            {
                UserName = HttpContext.Session.GetString("RegisterUsername"),
                Name = HttpContext.Session.GetString("RegisterName"),
                Email = HttpContext.Session.GetString("RegisterEmail"),
                CreatedDate = DateTime.Now,
                EmailConfirmed = true
            };

            var password =
                HttpContext.Session.GetString("RegisterPassword");

            var result =
                await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                ViewBag.Error =
                    string.Join("<br/>",
                    result.Errors.Select(x => x.Description));

                return View();
            }

            await _userManager.AddToRoleAsync(user, "User");

            await _signInManager.SignInAsync(user, false);

            HttpContext.Session.Remove("OTP");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Vui lòng nhập email";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ViewBag.Error = "Email không tồn tại";
                return View();
            }

            var otp = new Random()
                .Next(100000, 999999)
                .ToString();

            HttpContext.Session.SetString(
                "ResetOTP",
                otp);

            HttpContext.Session.SetString(
                "ResetEmail",
                email);

            HttpContext.Session.SetString(
                "ResetOtpCreatedAt",
                DateTime.Now.ToString());

            await _emailService.SendEmailAsync(
                email,
                "Mã OTP đặt lại mật khẩu",
                $@"
        <h2>Đặt lại mật khẩu</h2>

        <p>Mã OTP của bạn là:</p>

        <h1>{otp}</h1>

        <p>OTP có hiệu lực trong 5 phút.</p>
        "
            );

            return RedirectToAction("VerifyResetOtp");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(
    string password,
    string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error =
                    "Mật khẩu nhập lại không khớp";

                return View();
            }

            var email =
                HttpContext.Session.GetString("ResetEmail");

            var user =
                await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ViewBag.Error = "Không tìm thấy tài khoản";
                return View();
            }

            var token =
                await _userManager.GeneratePasswordResetTokenAsync(user);

            var result =
                await _userManager.ResetPasswordAsync(
                    user,
                    token,
                    password);

            if (!result.Succeeded)
            {
                ViewBag.Error =
                    string.Join("<br>",
                    result.Errors.Select(x => x.Description));

                return View();
            }

            HttpContext.Session.Remove("ResetOTP");
            HttpContext.Session.Remove("ResetEmail");

            TempData["Success"] =
                "Đổi mật khẩu thành công";

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult VerifyResetOtp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VerifyResetOtp(string otp)
        {
            var sessionOtp =
                HttpContext.Session.GetString("ResetOTP");

            var createdAtString =
                HttpContext.Session.GetString("ResetOtpCreatedAt");

            if (string.IsNullOrEmpty(createdAtString))
            {
                ViewBag.Error = "OTP đã hết hạn";
                return View();
            }

            var createdAt =
                DateTime.Parse(createdAtString);

            if (DateTime.Now > createdAt.AddMinutes(5))
            {
                HttpContext.Session.Remove("ResetOTP");

                ViewBag.Error = "OTP đã hết hạn";

                return View();
            }

            if (otp != sessionOtp)
            {
                ViewBag.Error = "OTP không đúng";

                return View();
            }
            HttpContext.Session.Remove("ResetOTP");
            return RedirectToAction("ResetPassword");
        }
    }
}
