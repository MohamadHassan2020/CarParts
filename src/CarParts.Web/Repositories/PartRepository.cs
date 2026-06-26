using CarParts.Web.Data;
using CarParts.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CarParts.Web.Repositories;

public class PartRepository(AppDbContext db) : IPartRepository
{
    public async Task<(IReadOnlyList<Part> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Parts.AsNoTracking().OrderBy(p => p.Name);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<Part?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Parts.FindAsync(new object?[] { id }, ct);

    public async Task<bool> PartNumberExistsAsync(string partNumber, int? excludeId = null, CancellationToken ct = default) =>
        await db.Parts.AsNoTracking()
            .AnyAsync(p => p.PartNumber == partNumber && (excludeId == null || p.Id != excludeId), ct);

    public async Task AddAsync(Part part, CancellationToken ct = default)
    {
        db.Parts.Add(part);
        await db.SaveChangesAsync(ct);
    }

    public void SetConcurrencyToken(Part part, Guid rowVersion) =>
        db.Entry(part).Property(p => p.RowVersion).OriginalValue = rowVersion;

    public Task CommitAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);

    public async Task DeleteAsync(Part part, CancellationToken ct = default)
    {
        db.Parts.Remove(part);
        await db.SaveChangesAsync(ct);
    }
}
