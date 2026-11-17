using CinemaGo.Domain.Interfaces.External;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Infrastructure.Data;
using CinemaGo.Infrastructure.FileStorage;
using CinemaGo.Infrastructure.Identity;
using CinemaGo.Infrastructure.Notifications;
using CinemaGo.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
namespace CinemaGo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, string webRootPath)
    {
        services.AddDbContext<CinemaDbContext>(o => o.UseSqlServer(connectionString));

        services.AddIdentityCore<ApplicationUser>(o =>
        {
            o.User.RequireUniqueEmail = true;
            o.Password.RequiredLength = 8;
            o.Password.RequireDigit = true;
            o.Password.RequireLowercase = true;
            o.Password.RequireUppercase = true;
            o.Password.RequireNonAlphanumeric = false;
            o.Lockout.AllowedForNewUsers = true;
            o.Lockout.MaxFailedAccessAttempts = 5;
            o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<CinemaDbContext>()
            .AddDefaultTokenProviders();   // needed for the 6-digit e-mail codes and reset tokens

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IHallRepository, HallRepository>();
        services.AddScoped<ISeatRepository, SeatRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();

        services.AddScoped<IIdentityService, IdentityService>();

        services.AddSingleton<EmailQueue>();
        services.AddSingleton<IEmailSender, QueuedEmailSender>();
        // real Gmail SMTP when credentials exist, otherwise the logging fake
        services.AddSingleton<IEmailTransport>(sp => sp.GetRequiredService<IOptions<EmailSettings>>().Value.IsConfigured
            ? ActivatorUtilities.CreateInstance<SmtpEmailSender>(sp)
            : ActivatorUtilities.CreateInstance<FakeEmailSender>(sp));
        services.AddSingleton<IFileStorage>(new LocalFileStorage(webRootPath));
        return services;
    }
}