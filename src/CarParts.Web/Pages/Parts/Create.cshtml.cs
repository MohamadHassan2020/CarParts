using CarParts.Web.Data;
using CarParts.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarParts.Web.Pages.Parts;

public class CreateModel(AppDbContext db) : PageModel
{
    [BindProperty]
    public PartInputModel Input { get; set; } = new();

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var part = new Part
        {
            PartNumber = Input.PartNumber,
            Name       = Input.Name,
            Brand      = Input.Brand,
            Quantity   = Input.Quantity,
            Price      = Input.Price,
        };

        db.Parts.Add(part);
        await db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
