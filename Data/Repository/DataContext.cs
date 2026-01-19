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
    }
}
