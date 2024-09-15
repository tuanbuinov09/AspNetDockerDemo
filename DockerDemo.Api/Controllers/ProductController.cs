using DockerDemo.Api.Dtos;
using DockerDemo.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DockerDemo.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private readonly ProductManagementContext _dbContext;

    public ProductController(ProductManagementContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet(Name = "GetProducts")]
    public async Task<IEnumerable<Product>> Get()
    {
        return await _dbContext.Set<Product>().AsNoTracking().ToListAsync();
    }

    [HttpGet("{productID}", Name = "GetOneProduct")]
    public async Task<IActionResult> GetOne(Guid productID)
    {
        var product = await _dbContext.Set<Product>().AsNoTracking().FirstOrDefaultAsync(p => p.ID == productID);

        if (product == null)
        {
            return BadRequest("No product was found");
        }

        return Ok(product);
    }

    [HttpPost(Name = "AddProduct")]
    public async Task<IActionResult> Add(AddProductDto addProductDto)
    {
        var category = await _dbContext.Set<Category>().FirstOrDefaultAsync(p => p.ID == addProductDto.CategoryID);

        if (category == null)
        {
            return BadRequest("No category was found");
        }

        var newProductName = addProductDto.Name.Trim();
        var nameExisted = await _dbContext.Set<Product>().AnyAsync(p => p.Name == newProductName);

        if (nameExisted)
        {
            return BadRequest("Provided product name has been used");
        }

        var id = Guid.NewGuid();

        await _dbContext.Set<Product>().AddAsync(new Product
        {
            ID = id,
            Name = newProductName,
            Price = addProductDto.Price ?? 0,
            CategoryID = category.ID,
        });

        await _dbContext.SaveChangesAsync();

        return Ok(id);
    }

    [HttpPut("{productID}", Name = "UpdateProduct")]
    public async Task<IActionResult> Update(Guid productID, UpdateProductDto updateProductDto)
    {
        var product = await _dbContext.Set<Product>().FirstOrDefaultAsync(p => p.ID == productID);

        if (product == null)
        {
            return BadRequest("No product was found");
        }

        var category = await _dbContext.Set<Category>().FirstOrDefaultAsync(p => p.ID == updateProductDto.CategoryID);

        if (category == null)
        {
            return BadRequest("No category was found");
        }

        var newProductName = updateProductDto.Name.Trim();
        var nameExisted = await _dbContext.Set<Product>().AnyAsync(p => p.Name == newProductName
                                                                     && p.ID != productID);

        if (nameExisted)
        {
            return BadRequest("Provided product name has been used");
        }

        product.Name = newProductName;
        product.Price = updateProductDto.Price ?? 0;
        product.CategoryID = category.ID;

        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{productID}", Name = "DeleteProduct")]
    public async Task<IActionResult> Delete(Guid productID)
    {
        var product = await _dbContext.Set<Product>().FirstOrDefaultAsync(p => p.ID == productID);

        if (product == null)
        {
            return BadRequest("No product was found");
        }

        _dbContext.Set<Product>().Remove(product);

        await _dbContext.SaveChangesAsync();

        return Ok();
    }
}
