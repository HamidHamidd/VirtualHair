using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VirtualHair.Models;

namespace VirtualHair.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

public DbSet<VirtualHair.Models.Hairstyle> Hairstyle { get; set; } = default!;

public DbSet<VirtualHair.Models.Fade> Fade { get; set; } = default!;
}
