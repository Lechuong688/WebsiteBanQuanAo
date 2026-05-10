using Data.Entity;
using Data.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Data.Service;

namespace WebBanQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ChatController : Controller
    {
        private readonly DataContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(
            DataContext context,
            IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var sessions = await _context.ChatSession
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return View(sessions);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var session = await _context.ChatSession
                .FirstOrDefaultAsync(x => x.Id == id);

            if (session == null)
            {
                return NotFound();
            }

            var messages = await _context.ChatMessage
                .Where(x => x.ChatSessionId == id && !x.IsDeleted)
                .OrderBy(x => x.CreatedDate)
                .ToListAsync();

            ViewBag.Session = session;

            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> ReplyAdmin(int sessionId, string message)
        {
            var chat = new ChatMessageEntity
            {
                ChatSessionId = sessionId,
                SenderType = "ADMIN",
                Message = message,
                CreatedDate = DateTime.Now,
                IsDeleted = false
            };

            _context.ChatMessage.Add(chat);
            await _context.SaveChangesAsync();

            var currentTime = chat.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss");
            await _hubContext.Clients.Group(sessionId.ToString())
                 .SendAsync("ReceiveMessage", "ADMIN", message, currentTime);

            return Json(new { success = true });
        }
    }
}