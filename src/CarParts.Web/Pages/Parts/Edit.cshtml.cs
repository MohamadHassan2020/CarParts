using CarParts.Web.Models;
using CarParts.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarParts.Web.Pages.Parts;

public class EditModel(IPartService service) : PageModel
{
    [BindProperty]
    public PartInputModel Input { get; set; } = new();

    [BindProperty]
    public Guid RowVersion { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct = default)
    {
        var part = await service.GetByIdForReadAsync(id, ct);
        if (part is null) return NotFound();

        Input = new PartInputModel
        {
            PartNumber = part.PartNumber,
            Name       = part.Name,
            Brand      = part.Brand,
            Quantity   = part.Quantity,
            Price      = part.Price,
        };
        RowVersion = part.RowVersion;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await service.UpdateAsync(id, Input, RowVersion, ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return Page();
        }

        return RedirectToPage("Index");
    }
}
