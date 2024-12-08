using Vending.DataAccessLayer.Abstract;
using Vending.DataAccessLayer.Concrete;
using Vending.DataAccessLayer.Repositories;
using Vending.EntityLayer.Concrete;

namespace Vending.DataAccessLayer.EntityFramework
{
    public class EfProductDal : GenericRepository<Product>, IProductDal
    {
        public EfProductDal(VendingContext context) : base(context)
        {
        }
    }
}
