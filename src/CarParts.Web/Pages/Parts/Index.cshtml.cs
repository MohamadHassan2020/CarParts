using CarParts.Web.Models;
using CarParts.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarParts.Web.Pages.Parts;

public class IndexModel(IPartService service) : PageModel
{
    public const int PageSize = 20;

    public IReadOnlyList<Part> Parts { get; private set; } = [];
    public int TotalCount { get; private set; }
    public int CurrentPage { get; private set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public async Task OnGetAsync(int page = 1, CancellationToken ct = default)
    {
        CurrentPage = Math.Max(1, page);
        (Parts, TotalCount) = await service.GetPagedAsync(CurrentPage, PageSize, ct);
    }
}
