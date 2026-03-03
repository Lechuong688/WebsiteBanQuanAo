using Data.DTO.Banner;
using Data.DTO.Common;
using Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanQuanAo.Areas.Admin.Models;

namespace Data.Repository.Banner
{
    public interface IBannerRepository
    {
        Task<PagedResult<BannerListDTO>> GetList(int page, int pageSize, string? keyword, bool? status);
        Task<BannerEntity?> GetById(int id);
        BannerEntity Save(BannerUpdateDTO dto);
        void Delete(int id);
    }
}
