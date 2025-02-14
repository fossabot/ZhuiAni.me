using System.Diagnostics.CodeAnalysis;
using Me.Xfox.ZhuiAnime.Models;
using Microsoft.AspNetCore.Mvc;

namespace Me.Xfox.ZhuiAnime.Controllers;

/// <summary>Get items.</summary>
[ApiController, Route("api/items")]
public class ItemController : ControllerBase
{
    private ZAContext DbContext { get; init; }

    public ItemController(ZAContext dbContext)
    {
        DbContext = dbContext;
    }

    /// <summary>An item, like an anime, a manga, a episode in an anime, etc.</summary>
    public record ItemDto
    {
        public required uint Id { get; set; }
        public required uint CategoryId { get; set; }
        public required string Title { get; set; }
        public required IDictionary<string, string> Annotations { get; set; }
        public required uint? ParentItemId { get; set; }
        public required DateTimeOffset CreatedAt { get; set; }
        public required DateTimeOffset UpdatedAt { get; set; }
        public required string? ImageUrl { get; set; }

        [SetsRequiredMembers]
        public ItemDto(Item item)
        {
            Id = item.Id;
            CategoryId = item.CategoryId;
            Title = item.Title;
            Annotations = item.Annotations;
            ParentItemId = item.ParentItemId;
            CreatedAt = item.CreatedAt;
            UpdatedAt = item.UpdatedAt;
            ImageUrl = item.ImageUrl;
        }
    }

    /// <summary>List</summary>
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
            .OrderByDescending(x => x.UpdatedAt)
            .Select(i => new ItemDto(i))
            .ToListAsync();
    }

    public record CreateItemDto(
        uint CategoryId,
        string Title,
        IDictionary<string, string> Annotations,
        uint? ParentItemId
    );

    /// <summary>Create</summary>
    /// <param name="item"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status201Created)]
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

    protected async Task<Item> LoadItem(uint id) => await DbContext.Item.FindAsync(id)
        ?? throw new ZAError.ItemNotFound(id);

    /// <summary>Get</summary>
    /// <param name="id">item id</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ZAError.Has<ZAError.ItemNotFound>]
    public async Task<ItemDto> Get(uint id)
    {
        return new ItemDto(await LoadItem(id));
    }

    public record UpdateItemDto(
        uint? CategoryId,
        string? Title,
        IDictionary<string, string>? Annotations
    );

    /// <summary>Update</summary>
    /// <param name="id">item id</param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    [ZAError.Has<ZAError.ItemNotFound>]
    public async Task<ItemDto> Update(uint id, [FromBody] UpdateItemDto request)
    {
        var item = await LoadItem(id);
        item.CategoryId = request.CategoryId ?? item.CategoryId;
        item.Title = request.Title ?? item.Title;
        item.Annotations = request.Annotations ?? item.Annotations;
        await DbContext.SaveChangesAsync();
        return new ItemDto(item);
    }

    /// <summary>Delete</summary>
    /// <param name="id">item id</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ZAError.Has<ZAError.ItemNotFound>]
    public async Task<ItemDto> Delete(uint id)
    {
        var item = await LoadItem(id);
        DbContext.Item.Remove(item);
        await DbContext.SaveChangesAsync();
        return new ItemDto(item);
    }

    /// <summary>Get Child Items</summary>
    /// <param name="id">item id</param>
    /// <returns></returns>
    [HttpGet("{id}/items")]
    [ZAError.Has<ZAError.ItemNotFound>]
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
