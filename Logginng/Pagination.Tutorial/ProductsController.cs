using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Pagination.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private List<Product> productsTable = new List<Product>();
    public ProductsController()
    {
        for (int i = 1; i <= 100; i++)
        {
            productsTable.Add(new Product { Id = i, Name = "Product" + i, Price = 10.0m * i });
        }
    }

    [HttpGet]
    public IEnumerable<Product> Get(int page=1,int pageSize = 10)
    {
        var totalCount = productsTable.Count;
        var totalPages = (int)Math.Ceiling((decimal)totalCount/pageSize);
        var productsPerPage = productsTable
                                .Skip((page-1)*pageSize)
                                .Take(pageSize)
                                .ToList();
        return productsPerPage;
    }


}

public class Product
{
    public int Id { get; set; }
    public String? Name { get; set; }
    public decimal Price { get; set; }
}