using Data.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Data.Entity.Enums;

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
                .OrderBy(x => x.Name)
                .ToList();
        }

        public List<MasterDataEntity> GetSizes()
        {
            return _context.MasterData
                .Where(x => !x.IsDeleted && x.GroupId == 19)
                .OrderBy(x => x.Name)
                .ToList();
        }

        public List<MasterDataEntity> GetProductTypes()
        {
            return _context.MasterData
                .Where(x => !x.IsDeleted && x.GroupId == 0)
                .OrderBy(x => x.Name)
                .ToList();
        }

        public List<MasterDataEntity> GetCategories()
        {
            return _context.MasterData
                .Where(x =>
                    !x.IsDeleted &&
                    x.TypeId == (int)MasterDataType.Category
                )
                .OrderBy(x => x.GroupId)
                .ThenBy(x => x.Name)
                .ToList();
        }

        public MasterDataEntity? GetCategoryById(int id)
        {
            return _context.MasterData
                .FirstOrDefault(x =>
                    x.Id == id &&
                    !x.IsDeleted &&
                    x.TypeId == (int)MasterDataType.Category
                );
        }

        public List<MasterDataEntity> GetMainCategories()
        {
            return _context.MasterData
                .Where(x =>
                    !x.IsDeleted &&
                    x.GroupId == 0 &&
                    x.TypeId == (int)MasterDataType.Category
                )
                .OrderBy(x => x.Name)
                .ToList();
        }

        private string GenerateCode(string value)
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

        public void CreateCategory(string name, string code, string note, int parentId)
        {
            var finalCode = string.IsNullOrWhiteSpace(code)
                ? GenerateCode(name)
                : GenerateCode(code);

            var entity = new MasterDataEntity
            {
                Name = name,
                Code = finalCode,
                Note = note,
                GroupId = parentId,
                IsDeleted = false,
                CreatedDate = DateTime.Now
            };

            _context.MasterData.Add(entity);
            _context.SaveChanges();
        }

        public void UpdateCategory(int id, string name, string code, string note, int parentId)
        {
            var entity = _context.MasterData.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
            if (entity == null) return;

            entity.Name = name;
            entity.Code = string.IsNullOrWhiteSpace(code)
                ? GenerateCode(name)
                : GenerateCode(code);

            entity.Note = note;
            entity.GroupId = parentId;
            entity.UpdatedDate = DateTime.Now;

            _context.SaveChanges();
        }

        public void DeleteCategory(int id)
        {
            var entity = _context.MasterData
                .FirstOrDefault(x =>
                    x.Id == id &&
                    !x.IsDeleted &&
                    x.TypeId == (int)MasterDataType.Category
                );

            if (entity == null) return;

            bool hasChildren = _context.MasterData.Any(x =>
                !x.IsDeleted &&
                x.GroupId == id &&
                x.TypeId == (int)MasterDataType.Category
            );

            if (hasChildren)
            {
                return;
            }

            entity.IsDeleted = true;
            entity.UpdatedDate = DateTime.Now;

            _context.SaveChanges();
        }


        public List<MasterDataEntity> GetAttributes()
        {
            return _context.MasterData
                .Where(x =>
                    !x.IsDeleted &&
                    x.TypeId == (int)MasterDataType.Attribute
                )
                .OrderBy(x => x.GroupId)
                .ThenBy(x => x.Name)
                .ToList();
        }
        public void CreateAttribute(string name, string code, string note, int parentId)
        {
            var finalCode = string.IsNullOrWhiteSpace(code)
                ? GenerateCode(name)
                : GenerateCode(code);

            var entity = new MasterDataEntity
            {
                Name = name,
                Code = finalCode,
                Note = note,
                GroupId = parentId,
                TypeId = (int)MasterDataType.Attribute,
                IsDeleted = false,
                CreatedDate = DateTime.Now
            };

            _context.MasterData.Add(entity);
            _context.SaveChanges();
        }

        public void UpdateAttribute(int id, string name, string note, string code, int parentId)
        {
            var entity = _context.MasterData
                .FirstOrDefault(x =>
                    x.Id == id &&
                    !x.IsDeleted &&
                    x.TypeId == (int)MasterDataType.Attribute
                );

            if (entity == null) return;

            entity.Name = name;
            entity.Code = string.IsNullOrWhiteSpace(code)
                ? GenerateCode(name)
                : GenerateCode(code);
            entity.Note = note;
            entity.GroupId = parentId;
            entity.UpdatedDate = DateTime.Now;

            _context.SaveChanges();
        }

        public void DeleteAttribute(int id)
        {
            var entity = _context.MasterData
                .FirstOrDefault(x =>
                    x.Id == id &&
                    !x.IsDeleted &&
                    x.TypeId == (int)MasterDataType.Attribute
                );

            if (entity == null) return;

            bool hasChildren = _context.MasterData.Any(x =>
                !x.IsDeleted &&
                x.GroupId == id &&
                x.TypeId == (int)MasterDataType.Attribute
            );

            if (hasChildren)
            {
                return;
            }

            entity.IsDeleted = true;
            entity.UpdatedDate = DateTime.Now;

            _context.SaveChanges();
        }


        public List<MasterDataEntity> GetMainAttributes()
        {
            return _context.MasterData
                .Where(x =>
                    !x.IsDeleted &&
                    x.TypeId == (int)MasterDataType.Attribute &&
                    x.GroupId == 0
                )
                .OrderBy(x => x.Name)
                .ToList();
        }

        public MasterDataEntity? GetAttributeById(int id)
        {
            return _context.MasterData
                .FirstOrDefault(x =>
                    x.Id == id &&
                    !x.IsDeleted &&
                    x.TypeId == (int)MasterDataType.Attribute
                );
        }


    }
}
