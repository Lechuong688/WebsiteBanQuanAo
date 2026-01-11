using Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.Product
{
    public interface IProductRepository
    {
        IEnumerable<ProductEntity> GetAll();
        ProductEntity? GetById(int id);
    }
}
