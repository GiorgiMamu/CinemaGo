using CinemaGo.Domain.Constants;
using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Enums;
using CinemaGo.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace CinemaGo.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task InitializeAsync(IServiceProvider services, string? superAdminEmail, string? superAdminPassword, CancellationToken ct = default)
    {
        var db = services.GetRequiredService<CinemaDbContext>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        await db.Database.MigrateAsync(ct);
        await SeedRolesAsync(services);
        await SeedSuperAdminAsync(services, superAdminEmail, superAdminPassword, logger);
        await SeedSessionsAsync(db, ct);
    }

    private static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var name in new[] { RoleNames.SuperAdmin, RoleNames.Admin, RoleNames.Customer })
            if (!await roles.RoleExistsAsync(name))
                await roles.CreateAsync(new IdentityRole(name));
    }

    private static async Task SeedSuperAdminAsync(IServiceProvider services, string? email, string? password, ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("SeedData:SuperAdminEmail / SuperAdminPassword are not configured. SuperAdmin was not created.");
            return;
        }

        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        if (await users.FindByEmailAsync(email) is not null) return;

        var admin = new ApplicationUser { UserName = "superadmin", Email = email, EmailConfirmed = true };
        var created = await users.CreateAsync(admin, password);
        if (!created.Succeeded)
            throw new InvalidOperationException("SuperAdmin seeding failed: " + string.Join("; ", created.Errors.Select(e => e.Description)));

        await users.AddToRoleAsync(admin, RoleNames.SuperAdmin);
        logger.LogInformation("SuperAdmin account created: {Email}", email);
    }

     private static async Task SeedSessionsAsync(CinemaDbContext db, CancellationToken ct)
    {
        if (await db.Sessions.AnyAsync(ct)) return;

        var schedule = new (int MovieId, int HallId, int Hour, int Minute)[]
        {
            (1, 1, 18, 0), (5, 1, 21, 0), (3, 2, 17, 30), (2, 2, 20, 0), (6, 3, 19, 0)
        };
        var today = DateTime.UtcNow.Date;

        for (var day = 1; day <= 3; day++)
            foreach (var (movieId, hallId, hour, minute) in schedule)
                db.Sessions.Add(new Session
                {
                    MovieId = movieId,
                    HallId = hallId,
                    StartsAt = today.AddDays(day).AddHours(hour).AddMinutes(minute),
                    StandardPrice = 12m,
                    VipPrice = 20m,
                    Status = SessionStatus.Scheduled
                });

        await db.SaveChangesAsync(ct);
    }
}