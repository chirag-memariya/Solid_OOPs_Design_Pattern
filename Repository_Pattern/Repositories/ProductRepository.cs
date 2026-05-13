using Data;
using Models;

namespace Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicatinoDbContext _context;
    public ProductRepository(ApplicatinoDbContext context)
    {
            _context = context;
    }




    public void Delete(int productId)
    {
        var productInDb = _context.Products.Find(productId);
        if(productInDb != null)
        {
            _context.Remove(productInDb);
        }
    }

    public IEnumerable<Product> GetAll()
    {
        var allProducts = _context.Products;
        return allProducts;
    }

    public Product GetById(int productId)
    {
        var productInDb = _context.Products.Find(productId);
        return productInDb;
    }

    public void Insert(Product product)
    {
        _context.Products.Add(product);
    }

    public void Save()
    {
        _context.SaveChanges();
    }

    public void Update(Product product)
    {
        var productInDb = _context.Products.Find(product.Id);
        if(productInDb != null)
        {
            product.Name = product.Name;
        }
    }
}