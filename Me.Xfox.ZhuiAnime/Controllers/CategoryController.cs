using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Me.Xfox.ZhuiAnime.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Me.Xfox.ZhuiAnime.Controllers;

/// <summary>
/// Get category.
/// </summary>
[ApiController, Route("api/categories")]
public class CategoryController : ControllerBase
{
    private ZAContext DbContext { get; init; }

    public CategoryController(ZAContext dbContext)
    {
        DbContext = dbContext;
    }

    /// <summary>
    /// Category information.
    /// </summary>
    /// <param name="Id">id</param>
    /// <param name="Title">user-friendly name</param>
    public record CategoryDto(
        uint Id,
        string Title
    )
    {
        public CategoryDto(Category category) : this(category.Id, category.Title)
        {
        }
    }

    /// <summary>
    /// Get all categories.
    /// </summary>
    /// <returns>List of categories.</returns>
    [HttpGet]
    public async Task<IEnumerable<CategoryDto>> ListAsync()
    {
        return await DbContext.Category
            .OrderBy(a => a.Id)
            .Select(c => new CategoryDto(c))
            .ToListAsync();
    }

    /// <param name="Title">user-friendly name</param>
    public record CreateCategoryDto(string Title);

    /// <summary>
    /// Create a new category.
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), (int)HttpStatusCode.Created)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateCategoryDto category)
    {
        var categoryDb = new Category
        {
            Title = category.Title
        };
        await DbContext.Category.AddAsync(categoryDb);
        await DbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { category_id = categoryDb.Id }, new CategoryDto(categoryDb));
    }

    /// <summary>
    /// Get a category.
    /// </summary>
    /// <param name="id">category id</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<CategoryDto> Get(uint id)
    {
        var category = await DbContext.Category.FindAsync(id);
        if (category == null)
        {
            throw new ZhuiAnimeError.CategoryNotFound(id);
        }
        return new CategoryDto(category);
    }

    /// <summary>
    /// Update a category.
    /// </summary>
    /// <param name="id">category id</param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("{id}")]
    public async Task<CategoryDto> Update(uint id, [FromBody] CreateCategoryDto request)
    {
        var category = await DbContext.Category.FindAsync(id);
        if (category == null)
        {
            throw new ZhuiAnimeError.CategoryNotFound(id);
        }
        category.Title = request.Title;
        await DbContext.SaveChangesAsync();
        return new CategoryDto(category);
    }

    /// <summary>
    /// Delete a category.
    /// </summary>
    /// <param name="id">category id</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<CategoryDto> Delete(uint id)
    {
        var category = await DbContext.Category.FindAsync(id);
        if (category == null)
        {
            throw new ZhuiAnimeError.CategoryNotFound(id);
        }
        DbContext.Remove(category);
        await DbContext.SaveChangesAsync();
        return new CategoryDto(category);
    }

    /// <summary>
    /// An item, like an anime, a manga, a episode in an anime, etc.
    /// </summary>
    /// <param name="Id">id</param>
    /// <param name="CategoryId">the id of category this item belongs to</param>
    /// <param name="Title">original title of the item</param>
    /// <param name="Annotations">additional information</param>
    /// <param name="ParentItemId">the id of the parent item, if this item belongs to a parent item</param>
    public record ItemDto(
        uint Id,
        uint CategoryId,
        string Title,
        IDictionary<string, string> Annotations,
        uint? ParentItemId
    )
    {
        public ItemDto(Item item) : this(
            item.Id,
            item.CategoryId,
            item.Title,
            item.Annotations,
            item.ParentItemId)
        {
        }
    }

    /// <summary>
    /// Get a category's items.
    /// </summary>
    /// <param name="id">category id</param>
    /// <returns></returns>
    [HttpGet("{id}/items")]
    public async Task<IEnumerable<ItemDto>> GetItems(uint id)
    {
        var category = await DbContext.Category.FindAsync(id);
        if (category == null)
        {
            throw new ZhuiAnimeError.CategoryNotFound(id);
        }
        var items = await DbContext.Entry(category)
            .Collection(c => c.Items!)
            .Query()
            .OrderByDescending(i => i.Id)
            .Select(i => new ItemDto(i))
            .ToListAsync();
        return items;
    }
}
