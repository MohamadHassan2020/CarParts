using CarParts.Web.Models;
using CarParts.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarParts.Web.Pages.Parts;

public class DetailsModel(IPartService service) : PageModel
{
    public Part Part { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct = default)
    {
        var part = await service.GetByIdAsync(id, ct);
        if (part is null) return NotFound();

        Part = part;
        return Page();
    }
}
