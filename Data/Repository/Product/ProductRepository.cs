using Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.Product
{
    public class ProductRepository : IProductRepository
    {
        private readonly DataContext _context;
        public ProductRepository(DataContext context)
        {
            _context = context;
        }

        public IEnumerable<ProductEntity> GetAll()
        {
            return _context.Product
                       .Where(x => !x.IsDeleted)
                       .ToList();
        }

        public ProductEntity? GetById(int id)
        {
            return _context.Product
                           .FirstOrDefault(x => x.Id == id && !x.IsDeleted);
        }
    }
}
