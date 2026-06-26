using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CarParts.Web.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace CarParts.Web.Pages.Account;

public class LoginModel(IOptions<AdminSettings> options) : PageModel
{
    [BindProperty]
    public LoginInputModel Input { get; set; } = new();

    public string? ReturnUrl { get; private set; }

    public void OnGet(string? returnUrl = null) =>
        ReturnUrl = returnUrl ?? Url.Content("~/Parts");

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/Parts");

        if (!ModelState.IsValid)
            return Page();

        if (!ValidateCredentials(Input.Username, Input.Password))
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }

        var claims = new List<Claim> { new(ClaimTypes.Name, Input.Username) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return LocalRedirect(ReturnUrl);
    }

    private bool ValidateCredentials(string username, string password)
    {
        var settings = options.Value;
        if (!string.Equals(username, settings.Username, StringComparison.OrdinalIgnoreCase))
            return false;

        var storedHash = SHA256.HashData(Encoding.UTF8.GetBytes(settings.Password));
        var inputHash  = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return CryptographicOperations.FixedTimeEquals(storedHash, inputHash);
    }
}

public class LoginInputModel
{
    [System.ComponentModel.DataAnnotations.Required]
    public string Username { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
