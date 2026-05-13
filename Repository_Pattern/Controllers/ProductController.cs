using Data;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories;

namespace Controllers;

public class ProductController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    public ProductController(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    [HttpGet("allProducts")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProduct()
    {
        var allProducts = _productRepository.GetAll().ToArray();
        return Ok(allProducts);
    }
}