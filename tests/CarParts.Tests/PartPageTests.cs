using CarParts.Web.Data;
using CarParts.Web.Models;
using CarParts.Web.Pages.Parts;
using CarParts.Web.Repositories;
using CarParts.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CarParts.Tests;

public class PartPageTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly IPartService _service;

    public PartPageTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _service = new PartService(
            new PartRepository(_db, NullLogger<PartRepository>.Instance),
            NullLogger<PartService>.Instance);
    }

    public void Dispose() => _db.Dispose();

    // ── Index ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Index_ReturnsParts()
    {
        _db.Parts.AddRange(
            new Part { PartNumber = "P001", Name = "Filter", Brand = "Bosch", Quantity = 10, Price = 9.99m },
            new Part { PartNumber = "P002", Name = "Brake Pad", Brand = "Brembo", Quantity = 5, Price = 29.99m }
        );
        await _db.SaveChangesAsync();

        var page = new IndexModel(_service);
        await page.OnGetAsync();

        Assert.Equal(2, page.Parts.Count);
        Assert.Equal(2, page.TotalCount);
    }

    [Fact]
    public async Task Index_PageSizeLimitsParts()
    {
        for (var i = 1; i <= 25; i++)
            _db.Parts.Add(new Part { PartNumber = $"P{i:000}", Name = $"Part {i}", Brand = "Brand", Price = 1m });
        await _db.SaveChangesAsync();

        var page = new IndexModel(_service);
        await page.OnGetAsync(page: 1);

        Assert.Equal(IndexModel.PageSize, page.Parts.Count);
        Assert.Equal(25, page.TotalCount);
        Assert.Equal(2, page.TotalPages);
    }

    // ── Create ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_SavesPartAndRedirects()
    {
        var page = new CreateModel(_service);
        page.Input = new PartInputModel { PartNumber = "P003", Name = "Spark Plug", Brand = "NGK", Quantity = 20, Price = 4.50m };

        var result = await page.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal(1, await _db.Parts.CountAsync());
    }

    [Fact]
    public async Task Create_InvalidModel_ReturnsPage()
    {
        var page = new CreateModel(_service);
        page.ModelState.AddModelError("Input.Name", "Required");
        page.Input = new PartInputModel();

        var result = await page.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal(0, await _db.Parts.CountAsync());
    }

    [Fact]
    public async Task Create_DuplicatePartNumber_ReturnsPageWithError()
    {
        _db.Parts.Add(new Part { PartNumber = "P001", Name = "Filter", Brand = "Bosch", Price = 9.99m });
        await _db.SaveChangesAsync();

        var page = new CreateModel(_service);
        page.Input = new PartInputModel { PartNumber = "P001", Name = "Other", Brand = "Other", Price = 5m };

        var result = await page.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(page.ModelState.IsValid);
        Assert.Equal(1, await _db.Parts.CountAsync());
    }

    // ── Delete ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_RemovesPartAndRedirects()
    {
        var part = new Part { PartNumber = "P004", Name = "Belt", Brand = "Gates", Quantity = 3, Price = 15.00m };
        _db.Parts.Add(part);
        await _db.SaveChangesAsync();

        var page = new DeleteModel(_service);
        var result = await page.OnPostAsync(part.Id);

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal(0, await _db.Parts.CountAsync());
    }

    [Fact]
    public async Task Delete_Get_NotFoundForMissingId()
    {
        var page = new DeleteModel(_service);
        var result = await page.OnGetAsync(999);

        Assert.IsType<NotFoundResult>(result);
    }

    // ── Edit ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Edit_Get_NotFoundForMissingId()
    {
        var page = new EditModel(_service);
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

        var page = new EditModel(_service);
        page.Input = new PartInputModel { PartNumber = "P005", Name = "Wiper Blade", Brand = "Bosch", Quantity = 8, Price = 13.00m };
        page.RowVersion = part.RowVersion;

        var result = await page.OnPostAsync(part.Id);

        Assert.IsType<RedirectToPageResult>(result);
        var updated = await _db.Parts.FindAsync(part.Id);
        Assert.Equal("Wiper Blade", updated!.Name);
        Assert.Equal(13.00m, updated.Price);
    }

    [Fact]
    public async Task Edit_InvalidModel_ReturnsPage()
    {
        var part = new Part { PartNumber = "P006", Name = "Wiper", Brand = "Bosch", Price = 12.00m };
        _db.Parts.Add(part);
        await _db.SaveChangesAsync();

        var page = new EditModel(_service);
        page.ModelState.AddModelError("Input.Name", "Required");
        page.Input = new PartInputModel();

        var result = await page.OnPostAsync(part.Id);

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task Edit_DuplicatePartNumber_ReturnsPageWithError()
    {
        _db.Parts.AddRange(
            new Part { PartNumber = "P007", Name = "Belt", Brand = "Gates", Price = 10m },
            new Part { PartNumber = "P008", Name = "Filter", Brand = "Bosch", Price = 5m }
        );
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();

        var p007 = await _db.Parts.FirstAsync(p => p.PartNumber == "P007");
        var p008 = await _db.Parts.FirstAsync(p => p.PartNumber == "P008");
        _db.ChangeTracker.Clear();

        var page = new EditModel(_service);
        page.Input = new PartInputModel { PartNumber = "P008", Name = "Belt", Brand = "Gates", Price = 10m };
        page.RowVersion = p007.RowVersion;

        var result = await page.OnPostAsync(p007.Id);

        Assert.IsType<PageResult>(result);
        Assert.False(page.ModelState.IsValid);
    }

    // ── Details ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Details_Get_ReturnsCorrectPart()
    {
        var part = new Part { PartNumber = "P009", Name = "Oil Filter", Brand = "Mann", Quantity = 15, Price = 7.50m };
        _db.Parts.Add(part);
        await _db.SaveChangesAsync();

        var page = new DetailsModel(_service);
        var result = await page.OnGetAsync(part.Id);

        Assert.IsType<PageResult>(result);
        Assert.Equal("Oil Filter", page.Part.Name);
        Assert.Equal("P009", page.Part.PartNumber);
    }

    [Fact]
    public async Task Details_Get_NotFoundForMissingId()
    {
        var page = new DetailsModel(_service);
        var result = await page.OnGetAsync(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
