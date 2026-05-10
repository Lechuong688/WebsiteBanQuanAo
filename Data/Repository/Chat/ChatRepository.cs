using Data.Entity;
using Data.Repository.Chat;
using Microsoft.EntityFrameworkCore;
using Data.Service;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Repository.Chat
{
    public class ChatRepository : IChatRepository
    {
        private readonly DataContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatRepository(DataContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<ChatSessionEntity> GetOrCreateSessionAsync(string userId)
        {
            var session = await _context.ChatSession.FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);
            if (session == null)
            {
                session = new ChatSessionEntity
                {
                    UserId = userId,
                    Title = "Chat AI",
                    CreatedDate = DateTime.Now,
                    IsDeleted = false
                };
                _context.ChatSession.Add(session);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("NewSessionCreated");
            }
            return session;
        }

        public async Task<ChatMessageEntity> SaveMessageAsync(int sessionId, string senderType, string message)
        {
            var chatMsg = new ChatMessageEntity
            {
                ChatSessionId = sessionId,
                SenderType = senderType,
                Message = message,
                CreatedDate = DateTime.Now,
                IsDeleted = false
            };
            _context.ChatMessage.Add(chatMsg);
            await _context.SaveChangesAsync();
            return chatMsg;
        }

        public async Task<List<ChatMessageEntity>> GetSessionHistoryAsync(int sessionId)
        {
            return await _context.ChatMessage
                .Where(x => x.ChatSessionId == sessionId && !x.IsDeleted)
                .OrderBy(x => x.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<ProductEntity>> GetOutfitProductsAsync(string message, bool isOffice)
        {
            var relatedProducts = new List<ProductEntity>();

            var topQuery = _context.Product.Where(p => p.Name.ToLower().Contains("sơ mi") || p.Name.ToLower().Contains("polo") || p.Name.ToLower().Contains("áo"));
            var bottomQuery = _context.Product.Where(p => p.Name.ToLower().Contains("quần") || p.Name.ToLower().Contains("jeans") || p.Name.ToLower().Contains("tây") || p.Name.ToLower().Contains("kaki"));
            var accessoryQuery = _context.Product.Where(p => p.Name.ToLower().Contains("giày") || p.Name.ToLower().Contains("thắt lưng") || p.Name.ToLower().Contains("ví") || p.Name.ToLower().Contains("mũ") || p.Name.ToLower().Contains("nón"));

            message = message.ToLower();

            if (message.Contains("áo thun") || message.Contains("phông")) topQuery = _context.Product.Where(p => p.Name.ToLower().Contains("thun") || p.Name.ToLower().Contains("phông"));
            else if (message.Contains("sơ mi")) topQuery = _context.Product.Where(p => p.Name.ToLower().Contains("sơ mi"));
            else if (message.Contains("áo khoác") || message.Contains("hoodie")) topQuery = _context.Product.Where(p => p.Name.ToLower().Contains("khoác") || p.Name.ToLower().Contains("hoodie"));

            if (message.Contains("jeans") || message.Contains("bò")) bottomQuery = _context.Product.Where(p => p.Name.ToLower().Contains("jean") || p.Name.ToLower().Contains("bò"));
            else if (message.Contains("quần tây") || message.Contains("quần âu")) bottomQuery = _context.Product.Where(p => p.Name.ToLower().Contains("tây") || p.Name.ToLower().Contains("âu"));

            if (message.Contains("mũ") || message.Contains("nón")) accessoryQuery = _context.Product.Where(p => p.Name.ToLower().Contains("mũ") || p.Name.ToLower().Contains("nón"));
            else if (message.Contains("giày")) accessoryQuery = _context.Product.Where(p => p.Name.ToLower().Contains("giày"));
            else if (message.Contains("ví")) accessoryQuery = _context.Product.Where(p => p.Name.ToLower().Contains("ví"));

            if (isOffice)
            {
                bottomQuery = bottomQuery.Where(p => !p.Name.ToLower().Contains("short") && !p.Name.ToLower().Contains("đùi"));
            }

            var top = await topQuery.OrderBy(x => Guid.NewGuid()).FirstOrDefaultAsync();
            var bottom = await bottomQuery.OrderBy(x => Guid.NewGuid()).FirstOrDefaultAsync();
            var accessory = await accessoryQuery.OrderBy(x => Guid.NewGuid()).FirstOrDefaultAsync();

            if (top != null) relatedProducts.Add(top);
            if (bottom != null) relatedProducts.Add(bottom);
            if (accessory != null) relatedProducts.Add(accessory);

            return relatedProducts;
        }

        public async Task<List<ProductEntity>> GetHotProductsAsync(int count)
        {
            return await _context.Product.OrderByDescending(x => x.Id).Take(count).ToListAsync();
        }

        public async Task<List<ProductEntity>> SearchProductsAsync(string message, bool isSummer, bool isWinter, int count)
        {
            var query = _context.Product.AsQueryable();
            bool hasCategoryMatch = false;

            if (message.Contains("áo thun") || message.Contains("áo phông") || message.Contains("tee"))
            {
                query = query.Where(p => p.Name.ToLower().Contains("thun") || p.Name.ToLower().Contains("phông") || p.Name.ToLower().Contains("tee"));
                hasCategoryMatch = true;
            }
            else if (message.Contains("sơ mi"))
            {
                query = query.Where(p => p.Name.ToLower().Contains("sơ mi"));
                hasCategoryMatch = true;
            }
            else if (message.Contains("áo khoác") || message.Contains("jacket"))
            {
                query = query.Where(p => p.Name.ToLower().Contains("khoác") || p.Name.ToLower().Contains("jacket"));
                hasCategoryMatch = true;
            }
            else if (message.Contains("quần đùi") || message.Contains("quần short") || message.Contains("short"))
            {
                query = query.Where(p => p.Name.ToLower().Contains("short") || p.Name.ToLower().Contains("đùi"));
                hasCategoryMatch = true;
            }
            else if (message.Contains("quần tây") || message.Contains("quần âu"))
            {
                query = query.Where(p => p.Name.ToLower().Contains("tây") || p.Name.ToLower().Contains("âu"));
                hasCategoryMatch = true;
            }
            else if (message.Contains("quần jeans") || message.Contains("quần bò"))
            {
                query = query.Where(p => p.Name.ToLower().Contains("jean") || p.Name.ToLower().Contains("bò"));
                hasCategoryMatch = true;
            }

            if (!hasCategoryMatch)
            {
                string[] stopWords = { "tôi", "muốn", "mua", "tìm", "xem", "có", "không", "cho", "cái", "chiếc", "hello", "hi", "chào", "shop", "ơi", "ạ", "nặng", "cao", "kg", "m", "cm", "thì", "sao", "nhất", "để", "mặc", "mát", "đỡ" };
                var keywords = message.Split(new[] { ' ', ',', '.', '?' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Where(k => k.Length > 1 && !stopWords.Contains(k)).ToList();

                if (keywords.Any())
                {
                    query = query.Where(p => keywords.Any(k => p.Name.ToLower().Contains(k)));
                }
                else
                {
                    return new List<ProductEntity>();
                }
            }

            if (isSummer) query = query.Where(p => !p.Name.ToLower().Contains("len") && !p.Name.ToLower().Contains("hoodie") && !p.Name.ToLower().Contains("nỉ") && !p.Name.ToLower().Contains("áo khoác"));
            if (isWinter) query = query.Where(p => p.Name.ToLower().Contains("len") || p.Name.ToLower().Contains("hoodie") || p.Name.ToLower().Contains("nỉ") || p.Name.ToLower().Contains("áo khoác"));

            return await query.Take(count).ToListAsync();
        }

        public async Task<string> GetProductImageUrlAsync(int productId)
        {
            return await _context.Attachment
                .Where(a => a.EntityId == productId && a.EntityType == "Product" && a.IsDeleted != true && a.FilePath != null)
                .Select(a => a.FilePath.Trim())
                .FirstOrDefaultAsync() ?? "/uploads/no-image.png";
        }
    }
}