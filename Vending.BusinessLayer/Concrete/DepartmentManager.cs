using Vending.BusinessLayer.Abstract;
using Vending.DataAccessLayer.Abstract;
using Vending.EntityLayer.Concrete;

namespace Vending.BusinessLayer.Concrete
{
    public class DepartmentManager : IDepartmentService
    {
        private readonly IDepartmentDal _departmentDal;

        public DepartmentManager(IDepartmentDal departmentDal)
        {
            _departmentDal = departmentDal;
        }

        public void TDelete(Department entity)
        {
            _departmentDal.Delete(entity);
        }

        public Department TGetById(int id)
        {
            return _departmentDal.GetById(id);
        }

        public List<Department> TGetList()
        {
            return _departmentDal.GetAll();
        }

        public void TInsert(Department entity)
        {
            _departmentDal.Insert(entity);
        }

        public void TUpdate(Department entity)
        {
            _departmentDal.Update(entity);
        }
    }
}
