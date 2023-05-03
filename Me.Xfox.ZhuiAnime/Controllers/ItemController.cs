using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Me.Xfox.ZhuiAnime.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Me.Xfox.ZhuiAnime.Controllers;

/// <summary>
/// Get items.
/// </summary>
[ApiController, Route("api/items")]
public class ItemController : ControllerBase
{
    private ZAContext DbContext { get; init; }

    public ItemController(ZAContext dbContext)
    {
        DbContext = dbContext;
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
    /// Get all items.
    /// </summary>
    /// <remarks>
    /// This API will only return those are top-level, i.e. do not have a parent
    /// item. The result will be ordered by id descendingly.
    /// </remarks>
    /// <returns>List of items.</returns>
    [HttpGet]
    public async Task<IEnumerable<ItemDto>> ListAsync()
    {
        return await DbContext.Item
            .Where(i => i.ParentItemId == null)
            .OrderByDescending(a => a.Id)
            .Select(i => new ItemDto(i))
            .ToListAsync();
    }

    public record CreateItemDto(
        uint CategoryId,
        string Title,
        IDictionary<string, string> Annotations,
        uint? ParentItemId
    );

    /// <summary>
    /// Create a new item.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(ItemDto), (int)HttpStatusCode.Created)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateItemDto item)
    {
        var newItem = new Item
        {
            CategoryId = item.CategoryId,
            Title = item.Title,
            Annotations = item.Annotations,
            ParentItemId = item.ParentItemId
        };
        await DbContext.Item.AddAsync(newItem);
        await DbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = newItem.Id }, new ItemDto(newItem));
    }

    protected async Task<Item> LoadItem(uint id)
    {
        var item = await DbContext.Item.FindAsync(id);
        if (item == null)
        {
            throw new ZhuiAnimeError.ItemNotFound(id);
        }
        return item;
    }

    /// <summary>
    /// Get a item.
    /// </summary>
    /// <param name="id">item id</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ItemDto> Get(uint id)
    {
        return new ItemDto(await LoadItem(id));
    }

    public record UpdateItemDto(
        uint? CategoryId,
        string? Title,
        IDictionary<string, string>? Annotations
    );

    /// <summary>
    /// Update a item.
    /// </summary>
    /// <param name="id">item id</param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("{id}")]
    public async Task<ItemDto> Update(uint id, [FromBody] UpdateItemDto request)
    {
        var item = await LoadItem(id);
        item.CategoryId = request.CategoryId ?? item.CategoryId;
        item.Title = request.Title ?? item.Title;
        item.Annotations = request.Annotations ?? item.Annotations;
        await DbContext.SaveChangesAsync();
        return new ItemDto(item);
    }

    /// <summary>
    /// Delete a item.
    /// </summary>
    /// <param name="id">item id</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ItemDto> Delete(uint id)
    {
        var item = await LoadItem(id);
        DbContext.Item.Remove(item);
        await DbContext.SaveChangesAsync();
        return new ItemDto(item);
    }

    /// <summary>
    /// Get a item's child items.
    /// </summary>
    /// <param name="id">item id</param>
    /// <returns></returns>
    [HttpGet("{id}/items")]
    public async Task<IEnumerable<ItemDto>> GetChildItems(uint id)
    {
        var item = await LoadItem(id);
        return await DbContext.Item
            .Where(i => i.ParentItemId == id)
            .OrderBy(i => i.Id)
            .Select(i => new ItemDto(i))
            .ToListAsync();
    }
}