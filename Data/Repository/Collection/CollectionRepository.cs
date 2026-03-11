using Data.DTO.Product;
using Data.Entity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Data.Repository.Collection
{
    
    public class CollectionRepository : ICollectionRepository
    {
        private readonly DataContext _dataContext;
        private readonly IDatabaseSql _databaseSql;
        public CollectionRepository(DataContext dataContext, IDatabaseSql databaseSql)
        {
            _dataContext = dataContext;
            _databaseSql = databaseSql;
        }
        public List<CollectionEntity> GetAll()
        {
            return _dataContext.Collection
                .OrderByDescending(x => x.CreatedDate)
                               .ToList();
        }

        public CollectionEntity GetById(int id)
        {
            return _dataContext.Collection
                .FirstOrDefault(x => x.Id == id);
        }

        public void Save(CollectionEntity entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Code))
            {
                entity.Code = GenerateCode(entity.Name);
            }

            var now = DateTime.Now;

            bool isActive = true;

            if (entity.StartDate != null && entity.StartDate > now)
                isActive = false;

            if (entity.EndDate != null && entity.EndDate < now)
                isActive = false;

            if (entity.Id == 0)
            {
                entity.CreatedDate = now;
                entity.IsActive = isActive;

                _dataContext.Collection.Add(entity);
            }
            else
            {
                var dbEntity = _dataContext.Collection
                    .FirstOrDefault(x => x.Id == entity.Id);

                if (dbEntity == null) return;

                dbEntity.Name = entity.Name;
                dbEntity.Code = entity.Code;
                dbEntity.Note = entity.Note;
                dbEntity.StartDate = entity.StartDate;
                dbEntity.EndDate = entity.EndDate;
                dbEntity.IsActive = isActive;
                dbEntity.UpdatedBy = entity.UpdatedBy;
                dbEntity.UpdatedDate = now;
            }

            _dataContext.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _dataContext.Collection
                                     .FirstOrDefault(x => x.Id == id);

            if (entity == null) return;

            entity.IsActive = false;
            entity.UpdatedDate = DateTime.Now;

            _dataContext.Collection.Update(entity);
            _dataContext.SaveChanges();
        }

        public List<int> GetProductIds(int collectionId)
        {
            return _dataContext.ProductCollection
                .Where(x => x.CollectionId == collectionId)
                .Select(x => x.ProductId)
                .ToList();
        }

        public void SaveProducts(int collectionId, List<int> productIds)
        {
            var oldData = _dataContext.ProductCollection
                .Where(x => x.CollectionId == collectionId);

            _dataContext.ProductCollection.RemoveRange(oldData);

            if (productIds != null && productIds.Any())
            {
                var newData = productIds.Select(pid => new ProductCollectionEntity
                {
                    CollectionId = collectionId,
                    ProductId = pid
                });

                _dataContext.ProductCollection.AddRange(newData);
            }
            _dataContext.SaveChanges();
        }

        public string GenerateCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalized = value.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return Regex.Replace(
                    sb.ToString()
                      .Normalize(NormalizationForm.FormC)
                      .ToUpper(),
                    @"[^A-Z0-9]+",
                    "_"
                )
                .Trim('_');
        }
        public bool IsCodeExists(string code, int currentId = 0)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            return _dataContext.Collection
                .Any(x => x.Code == code && x.Id != currentId);
        }

        public void UpdateCollectionStatus()
        {
            var now = DateTime.Now;

            var collections = _dataContext.Collection.ToList();

            foreach (var item in collections)
            {
                bool shouldActive = true;

                if (item.StartDate != null && item.StartDate > now)
                    shouldActive = false;

                if (item.EndDate != null && item.EndDate < now)
                    shouldActive = false;

                if (item.IsActive != shouldActive)
                {
                    item.IsActive = shouldActive;
                    item.UpdatedDate = now;
                }
            }

            _dataContext.SaveChanges();
        }

        public CollectionEntity? GetByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            UpdateCollectionStatus();
            var now = DateTime.Now;

            return _dataContext.Collection
                .FirstOrDefault(x =>
                    x.Code == code &&
                    x.IsActive &&
                    (x.StartDate == null || x.StartDate <= now) &&
                    (x.EndDate == null || x.EndDate >= now)
                );
        }

        //public List<ProductListDTO> GetProductsByCollection(int collectionId)
        //{
        //    var productIds = _dataContext.ProductCollection
        //        .Where(x => x.CollectionId == collectionId)
        //        .Select(x => x.ProductId)
        //        .Distinct()
        //        .ToList();

        //    return (
        //        from p in _dataContext.Product
        //        join md in _dataContext.MasterData
        //            on p.TypeId equals md.Id
        //        where productIds.Contains(p.Id)
        //              && !p.IsDeleted
        //              && !md.IsDeleted
        //        select new ProductListDTO
        //        {
        //            Id = p.Id,
        //            Name = p.Name,
        //            Price = p.Price,
        //            Quantity = p.Quantity,
        //            Note = p.Note,

        //            TypeId = p.TypeId,
        //            TypeName = md.Name,

        //            Files = _dataContext.Attachment
        //                .Where(a =>
        //                    a.EntityId == p.Id &&
        //                    a.EntityType == "Product" &&
        //                    a.IsDeleted != true &&
        //                    a.FilePath != null)
        //                .Select(a => a.FilePath!)
        //                .ToList()
        //        }
        //    )
        //    .OrderByDescending(x => x.Id)
        //    .ToList();
        //}

        public async Task<List<ProductListDTO>> GetProductsByCollection(int collectionId)
        {
            var param = new List<SqlParameter>
    {
        new SqlParameter("@CollectionId", collectionId)
    };

            var result = await _databaseSql.ExecuteProcXmlToList<ProductListDTO>(
                "Product_GetByCollection",
                param
            );

            return result?.ToList() ?? new List<ProductListDTO>();
        }


        public async Task<(CollectionEntity collection, List<ProductListDTO> products)?>
GetCollectionForUser(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return null;

            var code = slug.Replace("-", "_").ToUpper();

            var collection = GetByCode(code);
            if (collection == null)
                return null;

            var products = await GetProductsByCollection(collection.Id);

            return (collection, products);
        }


    }
}
