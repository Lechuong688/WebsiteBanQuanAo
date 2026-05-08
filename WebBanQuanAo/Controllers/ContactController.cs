using Data.DTO.Contact;
using Data.Service.Auth;
using Microsoft.AspNetCore.Mvc;

namespace WebBanQuanAo.Controllers
{
    public class ContactController : Controller
    {
        private readonly IEmailService _emailService;

        public ContactController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Send(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            await _emailService.SendContactEmail(
                model.FullName,
                model.Email,
                model.Phone,
                model.Subject,
                model.Message
            );

            TempData["Success"] = "Gửi liên hệ thành công";

            return RedirectToAction("Index");
        }
    }
}