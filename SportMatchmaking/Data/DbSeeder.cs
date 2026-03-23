using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace SportMatchmaking.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SportMatchmakingContext>();

            if (!await context.Roles.AnyAsync(r => r.Name == "Admin"))
            {
                context.Roles.Add(new Role { Name = "Admin", Description = "System administrator" });
            }

            if (!await context.Roles.AnyAsync(r => r.Name == "User"))
            {
                context.Roles.Add(new Role { Name = "User", Description = "Normal user" });
            }

            await context.SaveChangesAsync();
        }
    }
}
