using BackOffice.Services;
using Shared.Context;
using Shared.models;

namespace BackOffice.Data;

public class UserSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var userService = scope.ServiceProvider
            .GetRequiredService<UserService>();

        var context = scope.ServiceProvider
            .GetRequiredService<MyDbContext>();

        if (context.Users.Any())
            return;

        await userService.CreateUser(new User
        {
            FirstName = "Admin",
            LastName = "Admin",
            Email = "admin@test.com",
            Role = "Admin",
            Phone= "0347241725",
            HiringDate = DateTime.UtcNow
        });
    }
}