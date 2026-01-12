namespace Examen_Cyber_M2_S1.Services;

using Examen_Cyber_M2_S1.Models;
using Microsoft.EntityFrameworkCore;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<PromoCode> PromoCodes { get; set; } = null!;
}

