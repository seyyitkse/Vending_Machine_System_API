using Vending.DataAccessLayer.Abstract;
using Vending.DataAccessLayer.Concrete;
using Vending.DataAccessLayer.Repositories;
using Vending.EntityLayer.Concrete;

namespace Vending.DataAccessLayer.EntityFramework
{
    public class EfAppUserDal : GenericRepository<AppUser>, IAppUserDal
    {
        public EfAppUserDal(VendingContext context) : base(context)
        {
        }
    }
}
