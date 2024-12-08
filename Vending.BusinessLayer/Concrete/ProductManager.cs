using Vending.BusinessLayer.Abstract;
using Vending.DataAccessLayer.Abstract;
using Vending.EntityLayer.Concrete;

namespace Vending.BusinessLayer.Concrete
{
    public class ProductManager:IProductService
    {
        private readonly IProductDal _productDal;
        public ProductManager(IProductDal productDal)
        {
            _productDal = productDal;
        }
        public void TDelete(Product entity)
        {
            _productDal.Delete(entity);
        }
        public Product TGetById(int id)
        {
            return _productDal.GetById(id);
        }
        public List<Product> TGetList()
        {
            return _productDal.GetAll();
        }
        public void TInsert(Product entity)
        {
            _productDal.Insert(entity);
        }
        public void TUpdate(Product entity)
        {
            _productDal.Update(entity);
        }
    }
}
