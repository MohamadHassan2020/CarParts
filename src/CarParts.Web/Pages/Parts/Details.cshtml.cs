using CarParts.Web.Data;
using CarParts.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarParts.Web.Pages.Parts;

public class DetailsModel(AppDbContext db) : PageModel
{
    public Part Part { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var part = await db.Parts.FindAsync(id);
        if (part is null) return NotFound();
        Part = part;
        return Page();
    }
}
