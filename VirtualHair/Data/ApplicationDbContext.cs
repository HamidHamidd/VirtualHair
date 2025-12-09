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

        // Социални таблици (Feed)
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Like> Likes => Set<Like>();
        public DbSet<Comment> Comments => Set<Comment>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ------------------------
            // ВАШИТЕ СЪЩЕСТВУВАЩИ МОДЕЛИ
            // ------------------------

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

            builder.Entity<UserHairstyle>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserHairstyle>()
                .HasOne(u => u.Hairstyle)
                .WithMany()
                .HasForeignKey(u => u.HairstyleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserHairstyle>()
                .HasOne(u => u.FacialHair)
                .WithMany()
                .HasForeignKey(u => u.FacialHairId)
                .OnDelete(DeleteBehavior.SetNull);

            // ------------------------
            // ⭐ ДОБАВЕНОТО (FEED SYSTEM) ⭐
            // ------------------------

            // Post → User
            builder.Entity<Post>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Post → Comments
            builder.Entity<Comment>()
                .HasOne<Post>()
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment → User
            builder.Entity<Comment>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Post → Likes
            builder.Entity<Like>()
                .HasOne<Post>()
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Like → User
            builder.Entity<Like>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
