using CarParts.Web.Models;
using CarParts.Web.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarParts.Web.Services;

public class PartService(IPartRepository repository, ILogger<PartService> logger) : IPartService
{
    public Task<(IReadOnlyList<Part> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default) =>
        repository.GetPagedAsync(page, pageSize, ct);

    public Task<Part?> GetByIdAsync(int id, CancellationToken ct = default) =>
        repository.GetByIdAsync(id, ct);

    public Task<Part?> GetByIdForReadAsync(int id, CancellationToken ct = default) =>
        repository.GetByIdForReadAsync(id, ct);

    public async Task<ServiceResult> CreateAsync(PartInputModel input, CancellationToken ct = default)
    {
        if (await repository.PartNumberExistsAsync(input.PartNumber, ct: ct))
        {
            logger.LogWarning("Create rejected — duplicate part number {PartNumber}", input.PartNumber);
            return ServiceResult.Fail($"Part number '{input.PartNumber}' already exists.");
        }

        try
        {
            await repository.AddAsync(Map(input), ct);
            logger.LogInformation("Part {PartNumber} created", input.PartNumber);
            return ServiceResult.Ok();
        }
        catch (DbUpdateException)
        {
            logger.LogWarning("DB constraint violation on create for part number {PartNumber}", input.PartNumber);
            return ServiceResult.Fail($"Part number '{input.PartNumber}' already exists.");
        }
    }

    public async Task<ServiceResult> UpdateAsync(int id, PartInputModel input, Guid rowVersion, CancellationToken ct = default)
    {
        var part = await repository.GetByIdAsync(id, ct);
        if (part is null)
        {
            logger.LogWarning("Update rejected — part {Id} not found", id);
            return ServiceResult.Fail("Part not found.");
        }

        if (await repository.PartNumberExistsAsync(input.PartNumber, excludeId: id, ct: ct))
        {
            logger.LogWarning("Update rejected — part number {PartNumber} already in use", input.PartNumber);
            return ServiceResult.Fail($"Part number '{input.PartNumber}' is already used by another part.");
        }

        part.PartNumber = input.PartNumber;
        part.Name       = input.Name;
        part.Brand      = input.Brand;
        part.Quantity   = input.Quantity;
        part.Price      = input.Price;

        try
        {
            await repository.UpdateAsync(part, rowVersion, ct);
            logger.LogInformation("Part {Id} updated", id);
            return ServiceResult.Ok();
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogWarning("Concurrency conflict updating part {Id}", id);
            return ServiceResult.Fail("The record was modified by another user. Refresh and try again.");
        }
        catch (DbUpdateException)
        {
            logger.LogWarning("DB constraint violation on update for part number {PartNumber}", input.PartNumber);
            return ServiceResult.Fail($"Part number '{input.PartNumber}' is already used by another part.");
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var part = await repository.GetByIdAsync(id, ct);
        if (part is null)
        {
            logger.LogWarning("Delete rejected — part {Id} not found", id);
            return ServiceResult.Fail("Part not found.");
        }

        await repository.DeleteAsync(part, ct);
        logger.LogInformation("Part {Id} deleted", id);
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
