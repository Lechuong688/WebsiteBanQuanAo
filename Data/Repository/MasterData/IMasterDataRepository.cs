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
        List<MasterDataEntity> GetCategories();
        MasterDataEntity? GetCategoryById(int id);
        List<MasterDataEntity> GetMainCategories();
        void CreateCategory(string name, string code, string note, int parentId);
        void UpdateCategory(int id, string name, string code, string note, int parentId);
        void DeleteCategory(int id);

        List<MasterDataEntity> GetAttributes();
        void CreateAttribute(string name, string code, string note, int parentId);
        void UpdateAttribute(int id, string name, string note, string code, int parentId);
        void DeleteAttribute(int id);
        List<MasterDataEntity> GetMainAttributes();
        MasterDataEntity? GetAttributeById(int id);
    }
}
