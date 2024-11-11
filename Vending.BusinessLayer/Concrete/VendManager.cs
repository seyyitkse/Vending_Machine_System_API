using Vending.BusinessLayer.Abstract;
using Vending.DataAccessLayer.Abstract;
using Vending.EntityLayer.Concrete;

namespace Vending.BusinessLayer.Concrete
{
    public class VendManager : IVendService
    {
        private readonly IVendDal _vendDal;

        public VendManager(IVendDal vendDal)
        {
            _vendDal = vendDal;
        }

        public void TDelete(Vend entity)
        {
            _vendDal.Delete(entity);
        }

        public Vend TGetById(int id)
        {
            return _vendDal.GetById(id);
        }

        public List<Vend> TGetList()
        {
            return _vendDal.GetAll();
        }

        public void TInsert(Vend entity)
        {
            _vendDal.Insert(entity);
        }

        public void TUpdate(Vend entity)
        {
            _vendDal.Update(entity);
        }
    }
}
