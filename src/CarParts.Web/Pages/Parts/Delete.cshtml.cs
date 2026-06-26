using CarParts.Web.Data;
using CarParts.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarParts.Web.Pages.Parts;

public class DeleteModel(AppDbContext db) : PageModel
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

    public async Task<IActionResult> OnPostAsync()
    {
        var part = await db.Parts.FindAsync(Part.Id);
        if (part is not null)
        {
            db.Parts.Remove(part);
            await db.SaveChangesAsync();
        }
        return RedirectToPage("Index");
    }
}
