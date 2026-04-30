using Data.Entity;
using Data.Repository;
using Data.Service.ChatBot;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebBanQuanAo.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly ChatBotService _chatBotService;
        private readonly DataContext _context;

        public ChatController(
            ChatBotService chatBotService,
            DataContext context)
        {
            _chatBotService = chatBotService;
            _context = context;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage(
            [FromBody] ChatRequest request)
        {
            if (request == null
                || string.IsNullOrWhiteSpace(
                    request.Message))
            {
                return BadRequest(new
                {
                    success = false,
                    message =
                    "Tin nhắn không được để trống."
                });
            }

            string userMessage =
                request.Message.Trim();

            string userMessageLower =
    NormalizeText(
        userMessage.ToLower());

            // ==========================================
            // CACHE CÂU CHÀO
            // ==========================================

            if (userMessageLower.Contains("hello")
                || userMessageLower.Contains("hi")
                || userMessageLower.Contains("chào"))
            {
                return Ok(new
                {
                    success = true,
                    reply =
                    "Chào bạn 👋 Mình là Vybe AI. Bạn đang tìm phong cách nào hôm nay?"
                });
            }

            // ==========================================
            // SYSTEM PROMPT
            // ==========================================

            string systemPrompt = @"
Bạn là Vybe AI của website thời trang VYBE.

Quy tắc:
- Xưng 'mình', gọi khách là 'bạn'
- Trả lời tự nhiên
- Ngắn gọn
- Chỉ tư vấn dựa trên sản phẩm được cung cấp
- Nếu khách hỏi mùa hè:
  ưu tiên đồ mỏng, thoáng, cotton, áo thun, sơ mi ngắn tay
- Nếu khách hỏi mùa đông:
  ưu tiên hoodie, len, áo khoác
- Không tự bịa sản phẩm
";

            // ==========================================
            // TÌM SẢN PHẨM
            // ==========================================

            string productContext = "";
            List<ProductEntity> relatedProducts = new List<ProductEntity>();
            try
            {
                string[] stopWords =
                {
                    "tôi",
                    "muốn",
                    "mua",
                    "tìm",
                    "xem",
                    "có",
                    "không",
                    "cho",
                    "cái",
                    "chiếc",
                    "hello",
                    "hi",
                    "chào",
                    "shop",
                    "ơi",
                    "ạ"
                };

                var keywords =
                    userMessageLower
                    .Split(new[]
                    {
                        ' ',
                        ',',
                        '.',
                        '?'
                    },
                    StringSplitOptions
                        .RemoveEmptyEntries)

                    .Where(k =>
                        k.Length > 1
                        && !stopWords.Contains(k))

                    .ToList();
                bool isSummer =
                    userMessageLower.Contains("mùa hè")
                    || userMessageLower.Contains("nóng")
                    || userMessageLower.Contains("mát");

                bool isWinter =
                    userMessageLower.Contains("mùa đông")
                    || userMessageLower.Contains("lạnh");
                if (keywords.Any())
                {
                    relatedProducts =
                        await _context.Product

                        .Where(p =>
                            keywords.Any(k =>
                                p.Name.ToLower()
                                    .Contains(k)))

                        .Take(3)

                        .ToListAsync();

                    if (relatedProducts.Any())
                    {
                        productContext =
                        "\nDanh sách sản phẩm:\n";
                    }
                    else
                    {
                        productContext =
                        "\nKhông có sản phẩm phù hợp.\n";
                    }
                }
                else
                {
                    var query =
    _context.Product.AsQueryable();

                    query =
                        query.Where(p =>
                            keywords.Any(k =>
                                p.Name.ToLower().Contains(k)));

                    if (isSummer)
                    {
                        query =
                            query.Where(p =>
                                !p.Name.ToLower().Contains("len")
                                && !p.Name.ToLower().Contains("hoodie")
                                && !p.Name.ToLower().Contains("nỉ")
                                && !p.Name.ToLower().Contains("áo khoác"));
                    }

                    if (isWinter)
                    {
                        query =
                            query.Where(p =>
                                p.Name.ToLower().Contains("len")
                                || p.Name.ToLower().Contains("hoodie")
                                || p.Name.ToLower().Contains("nỉ")
                                || p.Name.ToLower().Contains("áo khoác"));
                    }

                    relatedProducts =
                        await query
                            .Take(3)
                            .ToListAsync();

                    productContext =
                    "\nSản phẩm gợi ý:\n";
                }

                foreach (var p in relatedProducts)
                {
                    productContext +=
                    $"- {p.Name} | Giá: {p.Price:N0} VNĐ | Link: /Product/Detail/{p.Id}\n";
                }
            }
            catch
            {
                productContext =
                    "Không thể tải sản phẩm.";
            }

            // ==========================================
            // FINAL PROMPT
            // ==========================================

            string finalPrompt = $@"
{systemPrompt}

{productContext}

Khách hỏi:
{userMessage}
";

            // ==========================================
            // GỌI AI
            // ==========================================

            var reply =
                await _chatBotService
                    .AskAI(finalPrompt);

            return Ok(new
            {
                success = true,
                reply = reply,

                products = relatedProducts.Select(p => new
                {
                    id = p.Id,

                    name = p.Name,

                    price = p.Price,

                    image = _context.Attachment
    .Where(a =>
        a.EntityId == p.Id
        && a.EntityType == "Product"
        && a.IsDeleted != true
        && a.FilePath != null)
    .Select(a => a.FilePath.Trim()) // CHỈ LẤY ĐƯỜNG DẪN TỪ DB LÀ ĐỦ
    .FirstOrDefault() ?? "/images/default-product.png", // Thêm ảnh mặc định phòng trường hợp SP chưa có ảnh

                    link = $"/Product/Detail/{p.Id}"
                })
            });
        }
        private string NormalizeText(string text)
        {
            text = text.ToLower().Trim();

            // =========================================
            // XÓA KÝ TỰ THỪA
            // =========================================

            text = text.Replace("?", " ");
            text = text.Replace(".", " ");
            text = text.Replace(",", " ");
            text = text.Replace("!", " ");
            text = text.Replace("  ", " ");

            // =========================================
            // CHUẨN HÓA KHÔNG DẤU
            // =========================================

            text = text.Replace("ao", "áo");
            text = text.Replace("quan", "quần");
            text = text.Replace("mu", "mũ");
            text = text.Replace("non", "nón");
            text = text.Replace("giay", "giày");
            text = text.Replace("dep", "dép");
            text = text.Replace("that lung", "thắt lưng");
            text = text.Replace("tui", "túi");

            // =========================================
            // ÁO THUN / ÁO PHÔNG
            // =========================================

            text = text.Replace("áo phong", "áo thun");
            text = text.Replace("ao phong", "áo thun");
            text = text.Replace("áo phông", "áo thun");
            text = text.Replace("ao phong", "áo thun");
            text = text.Replace("tee", "áo thun");
            text = text.Replace("tshirt", "áo thun");
            text = text.Replace("t-shirt", "áo thun");
            text = text.Replace("áo tee", "áo thun");

            // =========================================
            // ÁO KHOÁC
            // =========================================

            text = text.Replace("ao khoac", "áo khoác");
            text = text.Replace("áo khoac", "áo khoác");
            text = text.Replace("khoac", "khoác");
            text = text.Replace("klhoac", "khoác");
            text = text.Replace("klhoacs", "khoác");
            text = text.Replace("jacket", "áo khoác");
            text = text.Replace("coat", "áo khoác");

            // =========================================
            // HOODIE
            // =========================================

            text = text.Replace("hodi", "hoodie");
            text = text.Replace("hoddie", "hoodie");
            text = text.Replace("hoodi", "hoodie");
            text = text.Replace("áo nỉ", "hoodie");

            // =========================================
            // SWEATER / LEN
            // =========================================

            text = text.Replace("sweater", "len");
            text = text.Replace("sweter", "len");
            text = text.Replace("áo len", "len");

            // =========================================
            // QUẦN
            // =========================================

            text = text.Replace("jean", "jeans");
            text = text.Replace("quan jean", "quần jeans");
            text = text.Replace("quần jean", "quần jeans");
            text = text.Replace("jogger", "quần jogger");
            text = text.Replace("cargo", "quần cargo");
            text = text.Replace("short", "quần short");
            text = text.Replace("sọt", "short");

            // =========================================
            // MŨ
            // =========================================

            text = text.Replace("mu luoi trai", "mũ lưỡi trai");
            text = text.Replace("nón kết", "mũ lưỡi trai");
            text = text.Replace("cap", "mũ");

            // =========================================
            // GIÀY
            // =========================================

            text = text.Replace("sneaker", "giày");
            text = text.Replace("giầy", "giày");
            text = text.Replace("shoes", "giày");

            // =========================================
            // MÙA HÈ
            // =========================================

            text = text.Replace("mua he", "mùa hè");
            text = text.Replace("nong", "nóng");
            text = text.Replace("mat", "mát");

            // =========================================
            // MÙA ĐÔNG
            // =========================================

            text = text.Replace("mua dong", "mùa đông");
            text = text.Replace("lanh", "lạnh");
            text = text.Replace("ret", "rét");

            // =========================================
            // SIZE
            // =========================================

            text = text.Replace("sz", "size");
            text = text.Replace("sai", "size");

            // =========================================
            // CHAT THƯỜNG
            // =========================================

            text = text.Replace("helo", "hello");
            text = text.Replace("hii", "hi");
            text = text.Replace("shop oi", "shop ơi");
            text = text.Replace("ad", "admin");

            // =========================================
            // LỖI GÕ TELEX PHỔ BIẾN
            // =========================================

            text = text.Replace("aos", "áo");
            text = text.Replace("quanf", "quần");
            text = text.Replace("mux", "mũ");
            text = text.Replace("hooduie", "hoodie");
            text = text.Replace("khoc", "khoác");
            text = text.Replace("phonng", "phông");
            text = text.Replace("thoon", "thun");

            // =========================================
            // XÓA KHOẢNG TRẮNG THỪA
            // =========================================

            while (text.Contains("  "))
            {
                text = text.Replace("  ", " ");
            }

            return text.Trim();
        }
    }
    
    public class ChatRequest
    {
        public string Message { get; set; }
    }
}