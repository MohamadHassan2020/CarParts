using CarParts.Web.Models;
using CarParts.Web.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CarParts.Web.Services;

public class PartService(IPartRepository repository) : IPartService
{
    public Task<(IReadOnlyList<Part> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default) =>
        repository.GetPagedAsync(page, pageSize, ct);

    public Task<Part?> GetByIdAsync(int id, CancellationToken ct = default) =>
        repository.GetByIdAsync(id, ct);

    public async Task<ServiceResult> CreateAsync(PartInputModel input, CancellationToken ct = default)
    {
        if (await repository.PartNumberExistsAsync(input.PartNumber, ct: ct))
            return ServiceResult.Fail($"Part number '{input.PartNumber}' already exists.");

        try
        {
            await repository.AddAsync(Map(input), ct);
            return ServiceResult.Ok();
        }
        catch (DbUpdateException)
        {
            return ServiceResult.Fail($"Part number '{input.PartNumber}' already exists.");
        }
    }

    public async Task<ServiceResult> UpdateAsync(int id, PartInputModel input, Guid rowVersion, CancellationToken ct = default)
    {
        var part = await repository.GetByIdAsync(id, ct);
        if (part is null) return ServiceResult.Fail("Part not found.");

        if (await repository.PartNumberExistsAsync(input.PartNumber, excludeId: id, ct: ct))
            return ServiceResult.Fail($"Part number '{input.PartNumber}' is already used by another part.");

        part.PartNumber = input.PartNumber;
        part.Name       = input.Name;
        part.Brand      = input.Brand;
        part.Quantity   = input.Quantity;
        part.Price      = input.Price;

        repository.SetConcurrencyToken(part, rowVersion);

        try
        {
            await repository.CommitAsync(ct);
            return ServiceResult.Ok();
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult.Fail("The record was modified by another user. Refresh and try again.");
        }
        catch (DbUpdateException)
        {
            return ServiceResult.Fail($"Part number '{input.PartNumber}' is already used by another part.");
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var part = await repository.GetByIdAsync(id, ct);
        if (part is null) return ServiceResult.Fail("Part not found.");

        await repository.DeleteAsync(part, ct);
        return ServiceResult.Ok();
    }

    private static Part Map(PartInputModel input) => new()
    {
        PartNumber = input.PartNumber,
        Name       = input.Name,
        Brand      = input.Brand,
        Quantity   = input.Quantity,
        Price      = input.Price,
    };
}
