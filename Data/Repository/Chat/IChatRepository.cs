using Data.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Data.Repository.Chat
{
    public interface IChatRepository
    {
        Task<ChatSessionEntity> GetOrCreateSessionAsync(string userId);
        Task<ChatMessageEntity> SaveMessageAsync(int sessionId, string senderType, string message, string? productsJson = null);
        Task<List<ChatMessageEntity>> GetSessionHistoryAsync(int sessionId);

        Task<List<ProductEntity>> GetOutfitProductsAsync(string message, bool isOffice);
        Task<List<ProductEntity>> GetHotProductsAsync(int count);
        Task<List<ProductEntity>> SearchProductsAsync(string message, bool isSummer, bool isWinter, int count);
        Task<string> GetProductImageUrlAsync(int productId);
    }
}