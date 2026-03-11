using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

            // ☢️ Ядрена опция: Чистим ТАБЛИЦИТЕ СЪС ЗАВИСИМОСТИ ПЪРВО (за да избегнем FK грешки)
            await context.Database.ExecuteSqlRawAsync(@"
                DELETE FROM SavedLooks;
                DELETE FROM UserPhotos;
                DELETE FROM UserHairstyles;
                DELETE FROM Likes;
                DELETE FROM Comments;
                DELETE FROM Posts;
                DELETE FROM Friendships;
                DELETE FROM Messages;
                DELETE FROM AspNetUserRoles; 
                DELETE FROM AspNetUserClaims; 
                DELETE FROM AspNetUserLogins; 
                DELETE FROM AspNetUserTokens; 
                DELETE FROM AspNetRoleClaims; 
                DELETE FROM AspNetRoles; 
                DELETE FROM AspNetUsers;");

            // 1. Прясно създаване на роли
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            // 2. Прясно създаване на Админ
            var adminEmail = "admin@virtualhair.com";
            var adminPassword = "Admin#12345";

            var admin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                LockoutEnabled = false // Никога няма да се заключва
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
            else
            {
                throw new Exception("CRITICAL ERROR: Failed to create admin after full wipe!");
            }
        }
    }
}