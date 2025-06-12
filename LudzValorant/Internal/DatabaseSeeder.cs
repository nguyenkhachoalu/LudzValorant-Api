using LudzValorant.DataContexts;
using LudzValorant.Entities;
using Microsoft.EntityFrameworkCore;
using BcryptNet = BCrypt.Net.BCrypt;

namespace LudzValorant.Internal
{
    public class DatabaseSeeder
    {
        public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            await context.Database.MigrateAsync();

            var rolesToSeed = new List<Role>
    {
        new Role { RoleCode = "ADMIN", RoleName = "Administrator" },
        new Role { RoleCode = "USER", RoleName = "Standard User" },
        new Role { RoleCode = "MOD", RoleName = "Moderator" },
        new Role { RoleCode = "CONTRIBUTOR", RoleName = "Contributor" }
    };

            foreach (var role in rolesToSeed)
            {
                if (!await context.Roles.AnyAsync(r => r.RoleCode == role.RoleCode))
                    context.Roles.Add(role);
            }

            await context.SaveChangesAsync();

            // ✅ Đọc cấu hình từ appsettings.json
            var adminEmail = config["AdminAccount:Email"];
            var adminUsername = config["AdminAccount:Username"];
            var adminPassword = config["AdminAccount:Password"];

            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    FullName = "Hoa Lư đẹp trai",
                    Username = adminUsername,
                    Email = adminEmail,
                    PasswordHash = BcryptNet.HashPassword(adminPassword),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
            }

            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleCode == "ADMIN");
            if (adminRole != null)
            {
                bool hasAdminRole = await context.UserRoles.AnyAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id);
                if (!hasAdminRole)
                {
                    context.UserRoles.Add(new UserRole
                    {
                        UserId = adminUser.Id,
                        RoleId = adminRole.Id
                    });
                    await context.SaveChangesAsync();
                }
            }
        }

    }
}
