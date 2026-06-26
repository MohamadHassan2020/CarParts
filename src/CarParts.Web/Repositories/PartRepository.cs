using CarParts.Web.Data;
using CarParts.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarParts.Web.Repositories;

public class PartRepository(AppDbContext db, ILogger<PartRepository> logger) : IPartRepository
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
        await db.Parts.FindAsync([id], ct);

    public async Task<bool> PartNumberExistsAsync(string partNumber, int? excludeId = null, CancellationToken ct = default) =>
        await db.Parts.AsNoTracking()
            .AnyAsync(p => p.PartNumber == partNumber && (excludeId == null || p.Id != excludeId), ct);

    public async Task AddAsync(Part part, CancellationToken ct = default)
    {
        db.Parts.Add(part);
        await db.SaveChangesAsync(ct);
        logger.LogDebug("Part {PartNumber} added with Id {Id}", part.PartNumber, part.Id);
    }

    public async Task UpdateAsync(Part part, Guid rowVersion, CancellationToken ct = default)
    {
        db.Entry(part).Property(p => p.RowVersion).OriginalValue = rowVersion;
        await db.SaveChangesAsync(ct);
        logger.LogDebug("Part {Id} updated", part.Id);
    }

    public async Task DeleteAsync(Part part, CancellationToken ct = default)
    {
        db.Parts.Remove(part);
        await db.SaveChangesAsync(ct);
        logger.LogDebug("Part {Id} deleted", part.Id);
    }
}
