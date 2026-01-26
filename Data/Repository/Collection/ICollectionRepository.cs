using Data.DTO.Product;
using Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.Collection
{
    public interface ICollectionRepository
    {
        List<CollectionEntity> GetAll();
        CollectionEntity? GetById(int id);
        void Save(CollectionEntity entity);
        void Delete(int id);
        List<int> GetProductIds(int collectionId);
        void SaveProducts(int collectionId, List<int> productIds);
        string GenerateCode(string value);
        bool IsCodeExists(string code, int currentId = 0);
        CollectionEntity? GetByCode(string code);
        List<ProductListDTO> GetProductsByCollection(int collectionId);
        (CollectionEntity collection, List<ProductListDTO> products)?
        GetCollectionForUser(string slug);


    }
}
