using CarParts.Web.Data;
using CarParts.Web.Repositories;
using CarParts.Web.Services;
using CarParts.Web.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AdminSettings>(builder.Configuration.GetSection("Admin"));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Parts");
});

var provider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=carparts.db";

    if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        options.UseSqlServer(conn);
    else
        options.UseSqlite(conn);
});

builder.Services.AddScoped<IAdminCredentialService, AdminCredentialService>();
builder.Services.AddScoped<IPartRepository, PartRepository>();
builder.Services.AddScoped<IPartService, PartService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
