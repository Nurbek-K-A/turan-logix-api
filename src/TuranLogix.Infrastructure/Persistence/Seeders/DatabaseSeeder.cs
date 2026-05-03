using Microsoft.EntityFrameworkCore;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Domain.Entities;
using TuranLogix.Domain.Enums;
using TuranLogix.Infrastructure.Persistence;

namespace TuranLogix.Infrastructure.Persistence.Seeders;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(TuranLogixDbContext context, IPasswordHasher passwordHasher)
    {
        await context.Database.MigrateAsync();

        if (!await context.Users.AnyAsync(u => u.Role == UserRole.Admin))
        {
            var admin = User.Create(
                fullName: "Администратор",
                email: "admin@turanlogix.kz",
                phoneNumber: "+77000000000",
                passwordHash: passwordHasher.Hash("Admin123!"),
                role: UserRole.Admin);
            admin.Verify();
            await context.Users.AddAsync(admin);
            await context.SaveChangesAsync();
        }
    }
}
