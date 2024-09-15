using DockerDemo.Api.Dtos;
using DockerDemo.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace DockerDemo.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ProductManagementContext _dbContext;

    private readonly IConnectionMultiplexer _redis;

    private const string CategoryCacheKey = "categories";

    public CategoryController(ProductManagementContext dbContext, IConnectionMultiplexer connectionMultiplexer)
    {
        _dbContext = dbContext;
        _redis = connectionMultiplexer;
    }

    [HttpGet(Name = "GetCategories")]
    public async Task<IEnumerable<Category>> Get()
    {
        var db = _redis.GetDatabase();

        var cachedCategories = await db.StringGetAsync(CategoryCacheKey);
        if (!string.IsNullOrEmpty(cachedCategories))
        {
            var categoriesFromCache = JsonSerializer.Deserialize<IEnumerable<Category>>(cachedCategories);
            return categoriesFromCache ?? Enumerable.Empty<Category>();
        }

        var categoriesFromDb = await _dbContext.Set<Category>()
                                               .OrderBy(c => c.Name)
                                               .AsNoTracking()
                                               .ToListAsync();

        var serializedCategories = JsonSerializer.Serialize(categoriesFromDb);
        await db.StringSetAsync(CategoryCacheKey, serializedCategories, expiry: TimeSpan.FromDays(1));

        return categoriesFromDb;
    }

    [HttpGet("{categoryID}", Name = "GetOneCategory")]
    public async Task<IActionResult> GetOne(Guid categoryID)
    {
        var category = await _dbContext.Set<Category>()
                                       .Include(c => c.Products)
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(c => c.ID == categoryID);

        if (category == null)
        {
            return BadRequest("No category was found");
        }

        return Ok(category);
    }

    [HttpGet("{categoryID}/products", Name = "GetProductsOfCategory")]
    public async Task<IActionResult> GetProductsOfCategory(Guid categoryID)
    {
        var products = await _dbContext.Set<Product>()
                                       .Where(c => c.CategoryID == categoryID)
                                       .AsNoTracking()
                                       .ToListAsync();

        return Ok(products);
    }

    [HttpPost(Name = "AddCategory")]
    public async Task<IActionResult> Add(AddCategoryDto addCategoryDto)
    {
        var newCategoryName = addCategoryDto.Name.Trim();
        var nameExisted = await _dbContext.Set<Category>().AnyAsync(c => c.Name == newCategoryName);

        if (nameExisted)
        {
            return BadRequest("Provided category name has been used");
        }

        var id = Guid.NewGuid();

        await _dbContext.Set<Category>().AddAsync(new Category
        {
            ID = id,
            Name = newCategoryName,
        });

        await _dbContext.SaveChangesAsync();

        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(CategoryCacheKey);

        return Ok(id);
    }

    [HttpPut("{categoryID}", Name = "UpdateCategory")]
    public async Task<IActionResult> Update(Guid categoryID, UpdateCategoryDto updateCategoryDto)
    {
        var category = await _dbContext.Set<Category>().FirstOrDefaultAsync(c => c.ID == categoryID);

        if (category == null)
        {
            return BadRequest("No category was found");
        }

        var newCategoryName = updateCategoryDto.Name.Trim();
        var nameExisted = await _dbContext.Set<Category>().AnyAsync(c => c.Name == newCategoryName
                                                                      && c.ID != categoryID);

        if (nameExisted)
        {
            return BadRequest("Provided category name has been used");
        }

        category.Name = newCategoryName;

        await _dbContext.SaveChangesAsync();

        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(CategoryCacheKey);

        return Ok();
    }

    [HttpDelete("{categoryID}", Name = "DeleteCategory")]
    public async Task<IActionResult> Delete(Guid categoryID)
    {
        var category = await _dbContext.Set<Category>().FirstOrDefaultAsync(c => c.ID == categoryID);

        if (category == null)
        {
            return BadRequest("No category was found");
        }

        var categoryHasProduct = await _dbContext.Set<Product>().AnyAsync(p => p.CategoryID == categoryID);

        if (categoryHasProduct)
        {
            return BadRequest("Please delete or move products of this category to another one");
        }

        _dbContext.Set<Category>().Remove(category);

        await _dbContext.SaveChangesAsync();

        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(CategoryCacheKey);

        return Ok();
    }
}
