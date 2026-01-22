using Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.MasterData
{
    public interface IMasterDataRepository
    {
        List<MasterDataEntity> GetColors();
        List<MasterDataEntity> GetSizes();
        List<MasterDataEntity> GetProductTypes();
    }
}
