using CarParts.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CarParts.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Part> Parts => Set<Part>();
}
