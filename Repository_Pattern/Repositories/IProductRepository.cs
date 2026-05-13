using Models;

namespace Repositories;
public interface IProductRepository
{
    IEnumerable<Product> GetAll();
    Product GetById(int productId);
    void Insert(Product product);
    void Update(Product product);
    void Delete(int productId);
    void Save();
}