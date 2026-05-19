using Data.Entity;
using Data.Repository;
using Data.Repository.Chat;
using Data.Service;
using Data.Service.ChatBot;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebBanQuanAo.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly ChatBotService _chatBotService;
        private readonly IChatRepository _chatRepo;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly DataContext _context;

        public ChatController(
            ChatBotService chatBotService,
            IChatRepository chatRepo,
            IHubContext<ChatHub> hubContext,
            DataContext context)
        {
            _chatBotService = chatBotService;
            _chatRepo = chatRepo;
            _hubContext = hubContext;
            _context = context;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { success = false, message = "Tin nhắn không được để trống." });
            }

            string userMessage = request.Message.Trim();
            string userMessageLower = NormalizeText(userMessage.ToLower());
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ChatSessionEntity session;

            if (request.SessionId.HasValue)
            {
                session = await _context.ChatSession
                    .FirstOrDefaultAsync(x => x.Id == request.SessionId.Value);
            }
            else
            {
                session = new ChatSessionEntity
                {
                    UserId = userId,
                    Title = userMessage,
                    CreatedDate = DateTime.Now,
                    IsDeleted = false
                };

                _context.ChatSession.Add(session);
                await _context.SaveChangesAsync();
            }
            string senderType = request.Mode == "ADMIN" ? "USER_ADMIN" : "USER_AI";

            var userChat = await _chatRepo.SaveMessageAsync(session.Id, senderType, userMessage);
            var userTime = userChat.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss");
            await _hubContext.Clients.Group(session.Id.ToString()).SendAsync("ReceiveMessage", userChat.SenderType, userMessage, userTime);

            string reply = "";
            var relatedProducts = new List<ProductEntity>();
            var productsResponse = new List<object>();
            if (request.Mode != "ADMIN")
            {
                var chatHistory = await _chatRepo.GetSessionHistoryAsync(session.Id);
                string productContext = "";

                try
                {
                    string userMessageLowerClean = userMessageLower.Replace("1m70", "").Replace("52kg", "");

                    bool isOutfitRequest = userMessageLowerClean.Contains("mặc gì") || userMessageLowerClean.Contains("phối đồ") || userMessageLowerClean.Contains("gợi ý") || userMessageLowerClean.Contains("văn phòng") || userMessageLowerClean.Contains("công sở") || userMessageLowerClean.Contains("đi tiệc");
                    bool isHotRequest = userMessageLowerClean.Contains("hot") || userMessageLowerClean.Contains("bán chạy") || userMessageLowerClean.Contains("trend") || userMessageLowerClean.Contains("mới nhất");
                    bool isNegativeRequest = userMessageLowerClean.Contains("cũ") || userMessageLowerClean.Contains("ế") || userMessageLowerClean.Contains("xấu") || userMessageLowerClean.Contains("lỗi thời");

                    if (isOutfitRequest)
                    {
                        bool isOffice = userMessageLowerClean.Contains("văn phòng") || userMessageLowerClean.Contains("công sở");

                        relatedProducts = await _chatRepo.GetOutfitProductsAsync(userMessageLowerClean, isOffice);

                        productContext = "\nSẢN PHẨM BẠN CÓ THỂ DÙNG ĐỂ TƯ VẤN (Ráp thành 1 outfit hoàn chỉnh):\n";
                    }
                    else if (isHotRequest || isNegativeRequest)
                    {
                        relatedProducts = await _chatRepo.GetHotProductsAsync(3);
                        productContext = isNegativeRequest
                            ? "\nKHÁCH ĐANG HỎI ĐỒ CŨ/Ế. Bạn BẮT BUỘC phải lảng tránh khéo léo sau đó GIỚI THIỆU MẤY MÓN HOT BÊN DƯỚI:\n"
                            : "\nĐÂY LÀ DANH SÁCH HÀNG HOT/BÁN CHẠY NHẤT. Hãy tự tin khoe và giới thiệu cho khách:\n";
                    }
                    else
                    {
                        bool isSummer = userMessageLowerClean.Contains("mùa hè") || userMessageLowerClean.Contains("nóng") || userMessageLowerClean.Contains("mát");
                        bool isWinter = userMessageLowerClean.Contains("mùa đông") || userMessageLowerClean.Contains("lạnh");

                        relatedProducts = await _chatRepo.SearchProductsAsync(userMessageLowerClean, isSummer, isWinter, 3);

                        if (relatedProducts.Count == 0)
                        {
                            productContext = "\nHIỆN TẠI KHÔNG CÓ SẢN PHẨM KHỚP TỪ KHÓA. Xin lỗi khách là shop hết hàng. TUYỆT ĐỐI KHÔNG tự tư vấn thêm món đó.\n";
                        }
                        else
                        {
                            productContext = "\nDanh sách sản phẩm phù hợp hiện có:\n";
                        }
                    }

                    foreach (var p in relatedProducts)
                    {
                        productContext += $"- Tên: {p.Name} | Giá: {p.Price:N0} VNĐ\n";
                    }
                }
                catch
                {
                    productContext = "Không tải được sản phẩm.";
                }

                string shopInfoContext = @"
- Tên shop: VYBE (Chuyên thời trang GenZ nam nữ).
- Địa chỉ (Map): 123 Đường Cầu Diễn, Phường Minh Khai, Quận Bắc Từ Liêm, Hà Nội.
- Email: supportvybevn@gmail.com
- Hotline (Zalo/Call): 0372.783.688
- Đổi trả: Miễn phí trong 7 ngày, giữ nguyên tem mác.
- Vận chuyển: Freeship đơn từ 500k. Dưới 500k phí ship 30k.
- Gặp Admin thật: Bấm nút 'Chat với Admin' màu xanh lá bên dưới, hoặc là nhìn trên thanh Menu chọn 'liên hệ' điền thông tin họ tên, email, số điện thoại, chủ đề, nội dung và gửi email trực tiếp cho admin, trong đó có thông tin của website và có cả map trong đó rồi.
";

                string systemPrompt = $@"Bạn là Vybe AI - Nhân viên Sale và CSKH xuất sắc nhất của website thời trang VYBE.

THÔNG TIN CỬA HÀNG:
{shopInfoContext}

DỮ LIỆU SẢN PHẨM HIỆN CÓ CỦA KHÁCH YÊU CẦU:
{productContext}

7 NGUYÊN TẮC VÀNG TRONG GIAO TIẾP BẮT BUỘC TUÂN THỦ:
1. NHÂN CÁCH: Xưng 'mình', gọi khách là 'bạn'. Thái độ năng động, nhiệt tình, lịch sự chuẩn GenZ.
2. KHÔNG LẶP LẠI GIAO DIỆN: TUYỆT ĐỐI KHÔNG gõ tên sản phẩm, giá tiền hay link vào câu trả lời của bạn. Giao diện đã hiển thị ảnh rồi. Chỉ nói: 'Mình gợi ý cho bạn mẫu dưới đây nha!'
3. KỸ NĂNG PHỐI ĐỒ (MIX & MATCH): NẾU khách nhờ phối đồ hoặc hỏi 'hợp với', hãy ráp các món trong danh sách thành 1 Set hoàn chỉnh (VD: Áo này đi chung với quần kia và đội chiếc mũ bên dưới là bao ngầu!).
4. KỸ NĂNG BÁN CHÉO (UPSELL): NẾU khách chỉ hỏi mua 1 món (VD: 'mua áo thun') và danh sách trả về toàn áo thun, hãy trả lời xong và HỎI MỞ ĐỂ CHỐT SALE: 'Bạn có muốn mình gợi ý thêm chiếc quần jeans hay mũ nào để mix cùng chiếc áo này cho chuẩn bài không?'. 
5. XỬ LÝ KHÔNG CÓ HÀNG: NẾU Dữ liệu báo KHÔNG CÓ SẢN PHẨM, phải xin lỗi ngay vì đã hết hàng hoặc không kinh doanh. TUYỆT ĐỐI KHÔNG tự bịa ra sản phẩm ảo.
6. XỬ LÝ ĐỒ CŨ/Ế: NẾU khách hỏi móc mỉa về đồ cũ, ế, xấu... KHÔNG ĐƯỢC thừa nhận. Lảng tránh vui vẻ và khoe ngay 'VYBE toàn hàng trend mới nhất thôi, ví dụ như mấy món đang làm mưa làm gió dưới đây nè!'.
7. XỬ LÝ CÂU HỎI RÁC (OFF-TOPIC): Nếu khách hỏi đồ ăn, toán học, thời tiết, chính trị... PHẢI từ chối khéo léo, hài hước và LÁI NGAY về việc mua sắm quần áo tại VYBE.

Hãy dựa vào câu chat của khách và Dữ liệu Sản phẩm để đưa ra câu trả lời thông minh nhất!
";

                reply = await _chatBotService.AskAI(systemPrompt, userMessage, chatHistory);

                foreach (var p in relatedProducts)
                {
                    var img = await _chatRepo.GetProductImageUrlAsync(p.Id);
                    productsResponse.Add(new
                    {
                        id = p.Id,
                        name = p.Name,
                        price = p.Price,
                        image = img,
                        link = $"/Product/Detail/{p.Id}"
                    });
                }

                var aiChat = await _chatRepo.SaveMessageAsync(session.Id, "AI", reply, Newtonsoft.Json.JsonConvert.SerializeObject(productsResponse));
                var aiTime = aiChat.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss");
                await _hubContext.Clients.Group(session.Id.ToString()).SendAsync("ReceiveMessage", "AI", reply, aiTime);
            }

            return Ok(new
            {
                success = true,
                reply = reply,
                sessionId = session.Id,
                products = productsResponse
            });
        }

        private string NormalizeText(string text)
        {
            text = text.ToLower().Trim();
            text = text.Replace("?", " ").Replace(".", " ").Replace(",", " ").Replace("!", " ").Replace("  ", " ");
            text = text.Replace("ao", "áo").Replace("quan", "quần").Replace("mu", "mũ").Replace("non", "nón").Replace("giay", "giày").Replace("dep", "dép").Replace("that lung", "thắt lưng").Replace("tui", "túi");
            text = text.Replace("áo phong", "áo thun").Replace("ao phong", "áo thun").Replace("áo phông", "áo thun").Replace("tee", "áo thun").Replace("tshirt", "áo thun").Replace("t-shirt", "áo thun").Replace("áo tee", "áo thun");
            text = text.Replace("ao khoac", "áo khoác").Replace("áo khoac", "áo khoác").Replace("khoac", "khoác").Replace("klhoac", "khoác").Replace("klhoacs", "khoác").Replace("jacket", "áo khoác").Replace("coat", "áo khoác");
            text = text.Replace("hodi", "hoodie").Replace("hoddie", "hoodie").Replace("hoodi", "hoodie").Replace("áo nỉ", "hoodie");
            text = text.Replace("sweater", "len").Replace("sweter", "len").Replace("áo len", "len");
            text = text.Replace("jean", "jeans").Replace("quan jean", "quần jeans").Replace("quần jean", "quần jeans").Replace("jogger", "quần jogger").Replace("cargo", "quần cargo").Replace("short", "quần short").Replace("sọt", "short");
            text = text.Replace("mu luoi trai", "mũ lưỡi trai").Replace("nón kết", "mũ lưỡi trai").Replace("cap", "mũ");
            text = text.Replace("sneaker", "giày").Replace("giầy", "giày").Replace("shoes", "giày");
            text = text.Replace("mua he", "mùa hè").Replace("nong", "nóng").Replace("mat", "mát");
            text = text.Replace("mua dong", "mùa đông").Replace("lanh", "lạnh").Replace("ret", "rét");
            text = text.Replace("sz", "size").Replace("sai", "size");
            text = text.Replace("helo", "hello").Replace("hii", "hi").Replace("shop oi", "shop ơi").Replace("ad", "admin");
            text = text.Replace("aos", "áo").Replace("quanf", "quần").Replace("mux", "mũ").Replace("hooduie", "hoodie").Replace("khoc", "khoác").Replace("phonng", "phông").Replace("thoon", "thun");
            while (text.Contains("  ")) { text = text.Replace("  ", " "); }
            return text.Trim();
        }

        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages(int? sessionId)
        {
            if (!sessionId.HasValue)
            {
                return Ok(new
                {
                    sessionId = 0,
                    messages = new List<object>()
                });
            }

            var messages =
                await _chatRepo.GetSessionHistoryAsync(sessionId.Value);

            var result = messages.Select(x => new
            {
                sender = x.SenderType,
                message = x.Message,
                products = x.ProductsJson
            });

            return Ok(new
            {
                sessionId = sessionId.Value,
                messages = result
            });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
        public string? Mode { get; set; }
        public int? SessionId { get; set; }
    }
}