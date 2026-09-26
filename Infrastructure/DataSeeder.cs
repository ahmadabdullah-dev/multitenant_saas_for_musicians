using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class DataSeeder
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public DataSeeder(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager
        )
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task Seed()
    {
        await SeedRoles();
        await SeedUsers();
    }

    public async Task SeedRoles()
    {
        var dbRoleNames = await _roleManager.Roles
            .Select(r => r.Name)
            .ToListAsync();

        var roles = new List<AppRole>()
        {
            new() { Name = "Admin" },
            new() { Name = "Trainer" },
            new() { Name = "Student" },

        };

        foreach (var role in roles)
        {
            if (!dbRoleNames.Contains(role.Name))
            {
                var result = await _roleManager.CreateAsync(role);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create role '{role.Name}': {errors}");
                }
            }
        }
    }

    public async Task SeedUsers()
    {
        var users = new List<(AppUser user, string role, string password)>()
        {
            (new() { FirstName = "Ahmad", LastName = "Abdullah", UserName = "abdullah", Email = "ahmad@test.com", EmailConfirmed = true }, "Admin", "Pa$$w0rd"),
            (new() { FirstName = "Big", LastName = "Ramy", UserName = "ramy", Email = "big@test.com", EmailConfirmed = true }, "Trainer", "Pa$$w0rd"),
            (new() { FirstName = "Taylor", LastName = "Swift", UserName = "swift", Email = "swift@test.com", EmailConfirmed = true }, "Student", "Pa$$w0rd"),

        };

        foreach (var (user, role, password) in users)
        {
            var existingUser = await _userManager.FindByEmailAsync(user.Email!);

            if (existingUser != null)
                continue;

            var createResult = await _userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create user '{user.UserName}': {errors}");
            }

            var roleResult = await _userManager.AddToRoleAsync(user, role);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to add user '{user.UserName}' to role '{role}': {errors}");
            }
        }
    }
}