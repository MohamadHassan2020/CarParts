using CarParts.Web.Models;

namespace CarParts.Web.Repositories;

public interface IPartRepository
{
    Task<(IReadOnlyList<Part> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<Part?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> PartNumberExistsAsync(string partNumber, int? excludeId = null, CancellationToken ct = default);
    Task AddAsync(Part part, CancellationToken ct = default);
    Task UpdateAsync(Part part, Guid rowVersion, CancellationToken ct = default);
    Task DeleteAsync(Part part, CancellationToken ct = default);
}
