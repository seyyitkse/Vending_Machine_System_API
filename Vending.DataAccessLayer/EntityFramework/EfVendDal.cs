using Vending.DataAccessLayer.Abstract;
using Vending.DataAccessLayer.Concrete;
using Vending.DataAccessLayer.Repositories;
using Vending.EntityLayer.Concrete;

namespace Vending.DataAccessLayer.EntityFramework
{
    public class EfVendDal : GenericRepository<Vend>,IVendDal
    {
        public EfVendDal(VendingContext context) : base(context)
        {
        }
    }
}
