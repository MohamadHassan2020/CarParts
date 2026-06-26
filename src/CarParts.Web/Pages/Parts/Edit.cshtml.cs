using CarParts.Web.Data;
using CarParts.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarParts.Web.Pages.Parts;

public class EditModel(AppDbContext db) : PageModel
{
    [BindProperty]
    public Part Part { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var part = await db.Parts.FindAsync(id);
        if (part is null) return NotFound();
        Part = part;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
            return Page();

        var part = await db.Parts.FindAsync(id);
        if (part is null) return NotFound();

        part.PartNumber = Part.PartNumber;
        part.Name       = Part.Name;
        part.Brand      = Part.Brand;
        part.Quantity   = Part.Quantity;
        part.Price      = Part.Price;

        await db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
