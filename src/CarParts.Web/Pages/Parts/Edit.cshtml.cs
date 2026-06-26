using CarParts.Web.Data;
using CarParts.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarParts.Web.Pages.Parts;

public class EditModel(AppDbContext db) : PageModel
{
    [BindProperty]
    public PartInputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var part = await db.Parts.FindAsync(id);
        if (part is null) return NotFound();

        Input = new PartInputModel
        {
            PartNumber = part.PartNumber,
            Name       = part.Name,
            Brand      = part.Brand,
            Quantity   = part.Quantity,
            Price      = part.Price,
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
            return Page();

        var part = await db.Parts.FindAsync(id);
        if (part is null) return NotFound();

        part.PartNumber = Input.PartNumber;
        part.Name       = Input.Name;
        part.Brand      = Input.Brand;
        part.Quantity   = Input.Quantity;
        part.Price      = Input.Price;

        await db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
