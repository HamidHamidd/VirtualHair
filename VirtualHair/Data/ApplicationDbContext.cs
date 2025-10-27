using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VirtualHair.Models;

namespace VirtualHair.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Hairstyle> Hairstyles => Set<Hairstyle>();
        public DbSet<FacialHair> FacialHairs => Set<FacialHair>();
        public DbSet<UserPhoto> UserPhotos => Set<UserPhoto>();
        public DbSet<SavedLook> SavedLooks => Set<SavedLook>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserPhoto>()
                .HasIndex(x => new { x.UserId, x.CreatedAt });

            builder.Entity<SavedLook>()
                .HasOne(x => x.UserPhoto)
                .WithMany()
                .HasForeignKey(x => x.UserPhotoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SavedLook>()
                .HasOne(x => x.Hairstyle)
                .WithMany()
                .HasForeignKey(x => x.HairstyleId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<SavedLook>()
                .HasOne(x => x.FacialHair)
                .WithMany()
                .HasForeignKey(x => x.FacialHairId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
