using Vending.DataAccessLayer.Abstract;
using Vending.DataAccessLayer.Concrete;
using Vending.DataAccessLayer.Repositories;
using Vending.EntityLayer.Concrete;

namespace Vending.DataAccessLayer.EntityFramework
{
    public class EfCategoryDal : GenericRepository<Category>, ICategoryDal
    {
        public EfCategoryDal(VendingContext context) : base(context)
        {
        }
    }
}
