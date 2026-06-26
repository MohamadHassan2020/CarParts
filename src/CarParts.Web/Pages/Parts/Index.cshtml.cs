using CarParts.Web.Data;
using CarParts.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarParts.Web.Pages.Parts;

public class IndexModel(AppDbContext db) : PageModel
{
    public IReadOnlyList<Part> Parts { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Parts = await db.Parts.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
    }
}
