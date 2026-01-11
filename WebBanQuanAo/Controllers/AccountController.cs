using Microsoft.AspNetCore.Mvc;

namespace WebBanQuanAo.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
