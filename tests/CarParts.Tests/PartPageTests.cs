using CarParts.Web.Data;
using CarParts.Web.Models;
using CarParts.Web.Pages.Parts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarParts.Tests;

public class PartPageTests : IDisposable
{
    private readonly AppDbContext _db;

    public PartPageTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
    }

    public void Dispose() => _db.Dispose();

    // ── Index ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Index_ReturnsAllParts()
    {
        _db.Parts.AddRange(
            new Part { PartNumber = "P001", Name = "Filter", Brand = "Bosch", Quantity = 10, Price = 9.99m },
            new Part { PartNumber = "P002", Name = "Brake Pad", Brand = "Brembo", Quantity = 5, Price = 29.99m }
        );
        await _db.SaveChangesAsync();

        var page = new IndexModel(_db);
        await page.OnGetAsync();

        Assert.Equal(2, page.Parts.Count);
    }

    // ── Create ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_SavesPartAndRedirects()
    {
        var page = new CreateModel(_db);
        page.Part = new Part { PartNumber = "P003", Name = "Spark Plug", Brand = "NGK", Quantity = 20, Price = 4.50m };

        var result = await page.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal(1, await _db.Parts.CountAsync());
    }

    [Fact]
    public async Task Create_InvalidModel_ReturnsPage()
    {
        var page = new CreateModel(_db);
        page.ModelState.AddModelError("Part.Name", "Required");
        page.Part = new Part();

        var result = await page.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal(0, await _db.Parts.CountAsync());
    }

    // ── Delete ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_RemovesPartAndRedirects()
    {
        var part = new Part { PartNumber = "P004", Name = "Belt", Brand = "Gates", Quantity = 3, Price = 15.00m };
        _db.Parts.Add(part);
        await _db.SaveChangesAsync();

        var page = new DeleteModel(_db);

        var result = await page.OnPostAsync(part.Id);

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal(0, await _db.Parts.CountAsync());
    }

    [Fact]
    public async Task Delete_Get_NotFoundForMissingId()
    {
        var page = new DeleteModel(_db);

        var result = await page.OnGetAsync(999);

        Assert.IsType<NotFoundResult>(result);
    }

    // ── Edit ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Edit_Get_NotFoundForMissingId()
    {
        var page = new EditModel(_db);

        var result = await page.OnGetAsync(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_UpdatesPartAndRedirects()
    {
        var part = new Part { PartNumber = "P005", Name = "Wiper", Brand = "Bosch", Quantity = 8, Price = 12.00m };
        _db.Parts.Add(part);
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();

        var page = new EditModel(_db);
        page.Part = new Part { Id = part.Id, PartNumber = "P005", Name = "Wiper Blade", Brand = "Bosch", Quantity = 8, Price = 13.00m };

        var result = await page.OnPostAsync(part.Id);

        Assert.IsType<RedirectToPageResult>(result);
        var updated = await _db.Parts.FindAsync(part.Id);
        Assert.Equal("Wiper Blade", updated!.Name);
        Assert.Equal(13.00m, updated.Price);
    }
}
