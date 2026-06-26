using CarParts.Web.Models;

namespace CarParts.Web.Services;

public interface IPartService
{
    Task<(IReadOnlyList<Part> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<Part?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Part?> GetByIdForReadAsync(int id, CancellationToken ct = default);
    Task<ServiceResult> CreateAsync(PartInputModel input, CancellationToken ct = default);
    Task<ServiceResult> UpdateAsync(int id, PartInputModel input, Guid rowVersion, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);
}
