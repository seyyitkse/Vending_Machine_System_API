using Vending.BusinessLayer.Abstract;
using Vending.DataAccessLayer.Abstract;
using Vending.EntityLayer.Concrete;

namespace Vending.BusinessLayer.Concrete
{
    public class TodoItemManager : ITodoItemService
    {
        private readonly ITodoItemDal _todoItemDal;

        public TodoItemManager(ITodoItemDal todoItemDal)
        {
            _todoItemDal = todoItemDal;
        }

        public void TDelete(TodoItem entity)
        {
            _todoItemDal.Delete(entity);
        }

        public TodoItem TGetById(int id)
        {
            return _todoItemDal.GetById(id);
        }

        public List<TodoItem> TGetList()
        {
            return _todoItemDal.GetAll();
        }

        public void TInsert(TodoItem entity)
        {
            _todoItemDal.Insert(entity);
        }

        public void TUpdate(TodoItem entity)
        {
            _todoItemDal.Update(entity);
        }
    }
}
