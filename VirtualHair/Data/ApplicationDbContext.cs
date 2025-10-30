using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VirtualHair.Models;

namespace VirtualHair.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Основни таблици
        public DbSet<Hairstyle> Hairstyles => Set<Hairstyle>();
        public DbSet<FacialHair> FacialHairs => Set<FacialHair>();

        // Потребителски таблици
        public DbSet<UserPhoto> UserPhotos => Set<UserPhoto>();
        public DbSet<SavedLook> SavedLooks => Set<SavedLook>();
        public DbSet<UserHairstyle> UserHairstyles => Set<UserHairstyle>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Индекси за по-бързо търсене на потребителски снимки
            builder.Entity<UserPhoto>()
                .HasIndex(x => new { x.UserId, x.CreatedAt });

            // Връзка между SavedLook и UserPhoto (Cascade delete)
            builder.Entity<SavedLook>()
                .HasOne(x => x.UserPhoto)
                .WithMany()
                .HasForeignKey(x => x.UserPhotoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Връзка между SavedLook и Hairstyle (SetNull при изтриване)
            builder.Entity<SavedLook>()
                .HasOne(x => x.Hairstyle)
                .WithMany()
                .HasForeignKey(x => x.HairstyleId)
                .OnDelete(DeleteBehavior.SetNull);

            // Връзка между SavedLook и FacialHair (SetNull при изтриване)
            builder.Entity<SavedLook>()
                .HasOne(x => x.FacialHair)
                .WithMany()
                .HasForeignKey(x => x.FacialHairId)
                .OnDelete(DeleteBehavior.SetNull);

            // Връзка между UserHairstyle и User (FK)
            builder.Entity<UserHairstyle>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Връзка между UserHairstyle и Hairstyle
            builder.Entity<UserHairstyle>()
                .HasOne(u => u.Hairstyle)
                .WithMany()
                .HasForeignKey(u => u.HairstyleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Връзка между UserHairstyle и FacialHair (по избор)
            builder.Entity<UserHairstyle>()
                .HasOne(u => u.FacialHair)
                .WithMany()
                .HasForeignKey(u => u.FacialHairId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
