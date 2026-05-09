using DataAccess.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Models.Entites;
using Models.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Seed
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            // Extract the managers from the service provider
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            var context = serviceProvider.GetRequiredService<AppDbContext>();

            // 1. Seed Roles
            string[] roles = { "Admin", "Cashier" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new Role { Name = roleName });
                }
            }

            // 2. Seed Master Admin User
            const string adminUsername = "master_admin";
            // Check if the admin already exists to avoid duplicates
            var existingAdmin = await userManager.FindByNameAsync(adminUsername);

            if (existingAdmin == null)
            {
                var adminUser = new User
                {
                    UserName = adminUsername,
                    Name = "System Administrator",
                    Email = "admin@restaurant.com",
                    EmailConfirmed = true,
                    BranchId = null
                };

                // Create the user with a default secure password
                var createResult = await userManager.CreateAsync(adminUser, "Admin@123");

                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            if (!context.Branches.Any())
            {
                var defaultBranch = new Branch
                {
                    Name = "Main Downtown Branch"
                };
                context.Branches.Add(defaultBranch);
                await context.SaveChangesAsync();

                context.BranchTables.AddRange(
                    new BranchTable { BranchId = defaultBranch.Id, TableNumber = 1, Status = TableStatus.Available },
                    new BranchTable { BranchId = defaultBranch.Id, TableNumber = 2, Status = TableStatus.Available }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
