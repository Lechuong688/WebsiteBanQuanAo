using Data.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository
{
    public class DataContext : IdentityDbContext<UserEntity, RoleEntity, string>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        public DbSet<MasterDataEntity> MasterData { get; set; }
        //public DbSet<RoleEntity> Role { get; set; }
        //public DbSet<UserEntity> User { get; set; }
        public DbSet<ProductEntity> Product { get; set; }
        public DbSet<ProductAttributeEntity> ProductAttribute { get; set; }
        public DbSet<AttachmentEntity> Attachment { get; set; }
        public DbSet<OrderEntity> Order { get; set; }
        public DbSet<OrderDetailEntity> OrderDetail { get; set; }
        public DbSet<CollectionEntity> Collection { get; set; }
        public DbSet<ProductCollectionEntity> ProductCollection { get; set; }
        public DbSet<BannerEntity> Banner { get; set; }
        public DbSet<DiscountEntity> Discount { get; set; }
        public DbSet<ProductDiscountEntity> ProductDiscount { get; set; }
        public DbSet<DiscountCodeEntity> DiscountCode { get; set; }
        public DbSet<ProductWishlistEntity> ProductWishlist { get; set; }
        public DbSet<ChatSessionEntity> ChatSession { get; set; }

        public DbSet<ChatMessageEntity> ChatMessage { get; set; }
        public DbSet<ProductReviewEntity> ProductReview { get; set; }

        public DbSet<ProductReviewReplyEntity> ProductReviewReply { get; set; }

    }
}
