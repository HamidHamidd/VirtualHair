using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace VirtualHair.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<VirtualHair.Data.ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var env = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();

            // 0. Еднократно изтриване на всички потребители и техни данни
            string flagPath = Path.Combine(env.ContentRootPath, "delete_users.flag");
            if (File.Exists(flagPath))
            {
                // За да избегнем FK проблеми, първо трием свързаните данни
                context.UserPhotos.RemoveRange(context.UserPhotos);
                context.SavedLooks.RemoveRange(context.SavedLooks);
                context.UserHairstyles.RemoveRange(context.UserHairstyles);
                context.Posts.RemoveRange(context.Posts);
                context.Likes.RemoveRange(context.Likes);
                context.Comments.RemoveRange(context.Comments);
                context.Messages.RemoveRange(context.Messages);
                context.Friendships.RemoveRange(context.Friendships);
                await context.SaveChangesAsync();

                var allUsers = await userManager.Users.ToListAsync();
                foreach (var user in allUsers)
                {
                    await userManager.DeleteAsync(user);
                }
                
                // Премахваме флага, за да запазим потребителите занапред
                try { File.Delete(flagPath); } catch {}
            }

            // 1. Прясно създаване на роли
            string[] roles = { "Admin", "User" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Прясно създаване на Админ
            var adminEmail = "admin@virtualhair.com";
            var adminPassword = "Admin#12345";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var admin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    LockoutEnabled = false
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
            else
            {
                // Уверяваме се, че ако админът съществува, той все още е в "Admin" ролята
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
