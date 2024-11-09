using Vending.DataAccessLayer.Abstract;
using Vending.DataAccessLayer.Concrete;
using Vending.DataAccessLayer.Repositories;
using Vending.EntityLayer.Concrete;

namespace Vending.DataAccessLayer.EntityFramework
{
    public class EfTodoItemDal : GenericRepository<TodoItem>, ITodoItemDal
    {
        public EfTodoItemDal(VendingContext context) : base(context)
        {
        }
    }
}
