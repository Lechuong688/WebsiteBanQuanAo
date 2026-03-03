using Microsoft.AspNetCore.Mvc;

namespace WebBanQuanAo.Controllers
{
    public class BannerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
