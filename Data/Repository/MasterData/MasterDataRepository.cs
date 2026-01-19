using Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.MasterData
{
    public class MasterDataRepository : IMasterDataRepository
    {
        private readonly DataContext _context;

        public MasterDataRepository(DataContext context)
        {
            _context = context;
        }

        public List<MasterDataEntity> GetColors()
        {
            return _context.MasterData
                .Where(x => !x.IsDeleted && x.GroupId == 18)
                .Select(x => new MasterDataEntity
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToList();
        }

        public List<MasterDataEntity> GetSizes()
        {
            return _context.MasterData
                .Where(x => !x.IsDeleted && x.GroupId == 19)
                .Select(x => new MasterDataEntity
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToList();
        }
    }
}
