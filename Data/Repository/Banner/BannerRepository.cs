using Data.DTO.Banner;
using Data.DTO.Common;
using Data.DTO.Product;
using Data.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanQuanAo.Areas.Admin.Models;

namespace Data.Repository.Banner
{
    public class BannerRepository : IBannerRepository
    {
        private readonly DataContext _context;
        private readonly IDatabaseSql _databaseSql;
        public BannerRepository(DataContext context, IDatabaseSql databaseSql)
        {
            _context = context;
            _databaseSql = databaseSql;
        }
        public async Task<PagedResult<BannerListDTO>> GetList(int page, int pageSize, string? keyword, bool? status)
        {
            var par = new List<SqlParameter>()
            {
                new SqlParameter("@Page", page),
                new SqlParameter("@PageSize", pageSize),
                new SqlParameter("@Keyword", (object?)keyword ?? DBNull.Value),
                new SqlParameter("@Status", (object?)status ?? DBNull.Value)
            };

            var result = await _databaseSql
                .ExecuteProcToList<BannerListDTO>("Banner_Admin_GetList", par);

            return new PagedResult<BannerListDTO>
            {
                Items = result?.ToList() ?? new List<BannerListDTO>(),
                Page = page,
                PageSize = pageSize,
                TotalItems = result?.FirstOrDefault()?.TotalRecord ?? 0,
            };
        }

        public async Task<BannerEntity?> GetById(int id)
        {
            return await _context.Banner
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        public BannerEntity Save(BannerUpdateDTO dto)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                BannerEntity banner;

                if (dto.Id > 0)
                {
                    banner = _context.Banner
                        .FirstOrDefault(x => x.Id == dto.Id);

                    if (banner == null)
                        throw new Exception("Banner không tồn tại");

                    banner.Title = dto.Title;
                    banner.SubTitle = dto.SubTitle;
                    banner.ButtonText = dto.ButtonText;
                    banner.Description = dto.Description;
                    banner.CollectionId = dto.CollectionId;
                    banner.DisplayOrder = dto.DisplayOrder;

                    bool isActive = dto.IsActive;

                    if (dto.CollectionId != null)
                    {
                        var collection = _context.Collection
                            .FirstOrDefault(x => x.Id == dto.CollectionId);

                        if (collection != null && collection.IsActive == false)
                        {
                            isActive = false;
                        }
                    }
                    banner.IsActive = isActive;
                    banner.UpdatedBy = dto.UserId;
                    banner.UpdatedDate = DateTime.Now;
                }
                else
                {
                    bool isActive = dto.IsActive;

                    if (dto.CollectionId != null)
                    {
                        var collection = _context.Collection
                            .FirstOrDefault(x => x.Id == dto.CollectionId);

                        if (collection != null && collection.IsActive == false)
                        {
                            isActive = false;
                        }
                    }

                    banner = new BannerEntity
                    {
                        Title = dto.Title,
                        SubTitle = dto.SubTitle,
                        ButtonText = dto.ButtonText,
                        Description = dto.Description,
                        CollectionId = dto.CollectionId,
                        DisplayOrder = dto.DisplayOrder,
                        IsActive = isActive,
                        CreatedBy = dto.UserId,
                        CreatedDate = DateTime.Now
                    };

                    _context.Banner.Add(banner);
                    _context.SaveChanges();
                }


                if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                {
                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/banners");

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    var fileName = Guid.NewGuid() + Path.GetExtension(dto.ImageFile.FileName);
                    var fullPath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        dto.ImageFile.CopyTo(stream);
                    }

                    var oldImages = _context.Attachment
                        .Where(x => x.EntityId == banner.Id
                                 && x.EntityType == "Banner"
                                 && x.IsDeleted != true);

                    foreach (var img in oldImages)
                    {
                        img.IsDeleted = true;
                        //img.UpdatedDate = DateTime.Now;
                        //img.UpdatedBy = dto.UserId;
                    }

                    var attachment = new AttachmentEntity
                    {
                        EntityId = banner.Id,
                        EntityType = "Banner",
                        FileName = fileName,
                        FilePath = "/uploads/banners/" + fileName,
                        IsDeleted = false,
                        CreatedBy = dto.UserId,
                        CreatedDate = DateTime.Now
                    };

                    _context.Attachment.Add(attachment);
                }

                _context.SaveChanges();
                transaction.Commit();

                return banner;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void Delete(int id)
        {
            var entity = _context.Banner
                                     .FirstOrDefault(x => x.Id == id);

            if (entity == null) return;

            entity.IsActive = false;
            entity.UpdatedDate = DateTime.Now;

            _context.Banner.Update(entity);
            _context.SaveChanges();
        }
    }
}
