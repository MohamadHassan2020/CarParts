using CarParts.Web.Data;
using CarParts.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarParts.Web.Pages.Parts;

public class CreateModel(AppDbContext db) : PageModel
{
    [BindProperty]
    public Part Part { get; set; } = new();

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        db.Parts.Add(Part);
        await db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
